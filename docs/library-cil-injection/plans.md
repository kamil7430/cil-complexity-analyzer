Problem nr 1: "wspólny licznik" nie jest tak prosty, jak wygląda w dokumentacji

W obecnym kodzie CreateGlobalCounterField tworzy nową klasę <GlobalCounterContainer> i nowe pole __InstructionCounter za każdym razem, gdy wywołasz InjectCilToStudentSolution na jakimś module. To znaczy, że jeśli osobno "opomiarujesz" Graphs.dll i osobno StudentSolution.dll, to każde z tych assembly dostanie swoje własne, niezależne pole statyczne — mimo że nazywają się tak samo. To nie będzie jeden wspólny licznik w pamięci, tylko dwa osobne liczniki, które nigdy się nie zsumują.

Żeby to naprawić, potrzebujesz osobnego, trzeciego assembly — czegoś w rodzaju CilComplexityAnalyzer.Counter.dll — które:

zawiera tylko klasę <GlobalCounterContainer> z polem __InstructionCounter,
jest budowane raz, niezależnie,
jest referencjonowane (przez module.ImportReference) przez każde assembly, które wstrzykujesz — zarówno Graphs.dll, jak i StudentSolution.dll.

Dzięki temu, gdy oba assembly są załadowane w tym samym procesie (tym samym AssemblyLoadContext), odwołują się do dokładnie tego samego pola statycznego. To jest fundament, więc proponuję to jako Krok 0 planu.

Plan implementacji — podział na etapy
Etap 1: Wydzielenie wspólnego licznika
Nowy mały projekt Counter (czysta klasa, zero logiki) — kompilowany raz, dołączany do obrazu kontenera.
Refaktoryzacja CilInstructionInjector: zamiast CreateGlobalCounterField tworzącego nowy typ, metoda powinna przyjmować ModuleDefinition i zwracać FieldReference zaimportowane z Counter.dll (module.ImportReference(counterFieldInfo)).
To zmienia trochę istniejący kod studenta-injectora, ale to jest zmiana zgodna z Twoim TODO "inject CIL in student code — Done" — trzeba go lekko poprawić, żeby korzystał ze wspólnego pola.
Etap 2: Static Library Rewriting dla Graphs.dll (Kategoria A)

To dokładnie ten sam mechanizm co dla kodu studenta (InjectCounter + RedirectBranches już masz gotowe i można je re-użyć 1:1), różnice są w kiedy i gdzie:

Wykonywane raz, przy budowaniu obrazu Dockera (nie per-uruchomienie testu).
Wejście: oryginalny plik Graphs.dll z dysku (nie z pamięci jak u studenta).
Dodanie referencji do Counter.dll w module Graphs.dll.
Przejście po wszystkich typach/metodach (ten sam kod co masz) i wstrzyknięcie inkrementacji.
Zapisanie zmodyfikowanego pliku z powrotem na dysk, podmiana w miejscu, z którego student/kompilator go referencuje.
Ważna pułapka: jeśli Graphs.dll jest podpisane (strong-named), po modyfikacji bajtkodu podpis stanie się nieważny. Trzeba albo: usunąć strong name, albo przepodpisać własnym kluczem deweloperskim, albo wyłączyć weryfikację podpisu w runtime kontenera. To warto rozstrzygnąć zanim zaczniesz kodować, bo wpływa na to, czy w ogóle referencje się załadują.
Etap 3: Proxy Wrapper dla BCL (Kategoria B)

To jest bardziej złożone niż Etap 2, bo nie modyfikujesz biblioteki docelowej — modyfikujesz wywołania w kodzie studenta, żeby wskazywały na Twój wrapper.

Budujesz osobną bibliotekę Wrappers.dll z metodami statycznymi o sygnaturach odpowiadających najpopularniejszym metodom BCL, które chcesz obsłużyć (np. List<T>.Sort, Linq.OrderBy, Dictionary<T,K>.Add itd.) — każda z nich liczy koszt i przekazuje wywołanie dalej do oryginału.
Tworzysz mapowanie (np. Dictionary<string, MethodReference>) łączące "sygnatura oryginalnej metody BCL" → "referencja do metody wrapper".
W CilInstructionInjector, przy przechodzeniu instrukcji studenckiego kodu, dodajesz drugi przebieg: szukasz instrukcji Call/Callvirt, sprawdzasz czy instr.Operand (jako MethodReference) pasuje do czegoś w mapowaniu, i jeśli tak — podmieniasz operand na zaimportowaną referencję do wrappera.
Kluczowa decyzja do podjęcia: jak wrapper liczy koszt? Stały koszt "+1 za wywołanie" jest najprostszy, ale niesprawiedliwy (sort O(n log n) vs O(1) liczy się tak samo). Wrapper ma jednak dostęp do rzeczywistej kolekcji w momencie wywołania — może np. odczytać .Count i doliczyć realistyczny koszt algorytmiczny (np. n * log2(n) dla sortowania). To wymaga osobnej tabeli "typ operacji → wzór kosztu", ale daje dużo bardziej wiarygodne wyniki.
Etap 4: Mechanizm abortu (TODO z kodu)

Skoro doliczasz instrukcje w pętlach, potrzebujesz sposobu na przerwanie nieskończonej pętli/zbyt kosztownego rozwiązania. Najprościej: przy każdej inkrementacji licznika dodać sprawdzenie if (__InstructionCounter > LIMIT) throw. To dodatkowe IL do wstrzyknięcia w tym samym miejscu co inkrementacja — warto zaplanować to razem z Etapem 1, żeby nie przerabiać InjectCounter dwa razy.

Etap 5: Testy jednostkowe (TODO z kodu)

Zanim zaczniesz Etap 2 i 3, warto mieć testy na to, co już działa (Etap 0/1 dla kodu studenta):

test na prostej metodzie bez rozgałęzień — sprawdzenie dokładnej liczby wstrzykniętych instrukcji,
test na metodzie z pętlą i skokiem warunkowym — sprawdzenie, czy RedirectBranches poprawnie przekierowuje cele skoków,
test na metodzie z blokiem try/catch — sprawdzenie granic exception handlerów.

To da Ci bezpieczną bazę, zanim dołożysz komplikację w postaci dwóch dodatkowych assembly (Counter, Wrappers) i podmiany wywołań.
