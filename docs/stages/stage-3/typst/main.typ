#import "@preview/rubber-article:0.5.2": *

#show: article

#set text(lang: "pl")
#set page(numbering: none)

#align(horizon)[
  #maketitle(
    title: [Dokument etapu 3: _Implementacja funkcjonalności \ podstawowej i raport postępów_],
    authors: ([Tytuł projektu: _Projekt i implementacja platformy testowej weryfikującej i profilującej rozwiązania programistyczne_ \ \ Autorzy: Kacper Grobel, Kamil Błażejczyk, Kornel Dzieża \ Promotorzy: mgr inż. Tomasz Herman, dr inż. Michał Dębski],),
  )

  #align(bottom)[
    #image("logo-mini.png")
    #align(center)[XX listopada 2026]
  ]
]

#pagebreak()

#counter(page).update(1)
#set page(numbering: "1")
#set ref(supplement: none)
#show figure: set block(breakable: true)

#heading(depth: 2, numbering: none, outlined: false)[Abstrakt]
TODO
/*
//AI proposition
Niniejszy dokument przedstawia stan implementacji platformy testowej na koniec trzeciego etapu projektu. Opisano w nim zrealizowane moduły, odstępstwa od architektury z dokumentu etapu 2 oraz wyniki pierwszych pomiarów liczby instrukcji CIL. Szczególną uwagę poświęcono modułowi instrumentacji bibliotek (`LibraryCilInjector`), którego realizacja okazała się trudniejsza od założonej: przedstawiono przyczynę problemu, dwie równolegle rozwijane ścieżki jego rozwiązania oraz ich aktualny stan. Dokument zawiera także raport z testów, zestawienie z harmonogramem i prognozę terminu ukończenia prac.
*/
#heading(depth: 2, numbering: none, outlined: false)[Historia zmian dokumentu]

#table(
  columns: (10%, 12.5%, 17.5%, 60%),
  align: horizon + center,
  stroke: black + 0.5pt,

  table.header([*Wersja*], [*Data*], [*Autorzy*], [*Opis zmian*]),

  [1.0], [XX.11.2026], [KG / KB / KD], [Wersja bazowa dokumentu],
)

#heading(depth: 2, numbering: none, outlined: false)[Spis treści]

#outline(title: none)

#pagebreak()

#heading(depth: 2, numbering: none, outlined: false)[Słowniczek nowych pojęć]

Pojęcia zdefiniowane w dokumentach etapów 1 i 2 pozostają w mocy. Poniżej zestawiono pojęcia wprowadzone w tym dokumencie.

#v(1.0em)
#table(
  columns: (auto, 70%),
  align: horizon,
  stroke: black + 0.5pt,

  table.header([*Pojęcie*], [*Definicja*]),

  [R2R (_ReadyToRun_)], [Format prekompilowanych bibliotek .NET zawierający obok metadanych i CIL także natywny kod maszynowy. Biblioteki w tym formacie (tzw. _mixed-mode_) nie mogą być zapisane przez Mono.Cecil.],
  [BCL (_Base Class Library_)], [Standardowa biblioteka klas platformy .NET (m.in. `System`, `System.Linq`, `System.Collections`).],
  [`System.Private.CoreLib`], [Podstawowa biblioteka środowiska uruchomieniowego .NET, w której fizycznie znajdują się m.in. `List<T>`, `Dictionary<TKey, TValue>` i `Array`.],
  [Zestaw referencyjny (_Reference Assembly_)], [Biblioteka zawierająca wyłącznie sygnatury typów i metod, bez właściwego kodu wykonawczego.],
  [WebAssembly (WASM)], [Binarny format kodu wykonywany przez maszynę wirtualną. W projekcie służy do uruchamiania aplikacji .NET z bibliotekami w postaci czystego IL.],
  [`wasmtime`], [Środowisko uruchomieniowe WebAssembly, w którym wykonywana jest zinstrumentowana aplikacja.],
  [`LibraryCilInjector`], [Moduł rozszerzający `CilInjector` o instrumentację bibliotek, z których korzysta kod studenta.],
  [Rozgrzewka (_warm-up_)], [Jednorazowe wykonanie mierzonego kodu przed właściwym pomiarem, aby nie wliczać do wyniku kosztu inicjalizacji.],
)

= Stan implementacji i repozytorium <stan-implementacji> // KAMIL dokonał korekty:D

Tabela @tabela-stan-pracy prezentuje aktualny stan prac. Wszystkie moduły zaprojektowane w dokumencie etapu 2 zostały wstępnie zaimplementowane, z wyjątkiem modułu `CilInjector`, którego realizacja została rozszerzona o osobny komponent `LibraryCilInjector`. W przypadku modułów `TestExecutor` oraz `Facade` możliwe są niewielkie zmiany w oparciu o opinię klienta. 

Zgodnie z wymaganiami laboratorium na tym etapie nie wszystkie moduły muszą być zintegrowane.

#figure(
  table(
    columns: (auto, 1fr, auto),
    align: horizon,
    stroke: black + 0.5pt,
  
    table.header([*Projekt*], [*Odpowiedzialność*], [*Status*]),
  
    [`CilComplexityAnalyzer.Facade`], [Atrybuty, klasy bazowe `TestSuiteBase` i `TestCaseBase`, generator kodu.], [konsul- \ towany],
    [`CilComplexityAnalyzer.TestExecutor.Contract`], [Typy wymiany danych: `TestSuite`, `TestCase`, `TestSettings`, `TestResult`.], [gotowy],
    [`CilComplexityAnalyzer.TestExecutor`], [Silnik: `StaticAnalyzer`, `Compiler`, `Executor`.], [w trakcie],
    [`CilComplexityAnalyzer.ContainerWorker`], [Uruchamianie testów w środowisku izolowanym.], [gotowy],
    [`CilInjection`], [Instrumentacja kodu studenta oraz jego zależności w celu liczenia złożoności kodu.], [w trakcie],
  ),
  caption: "Zestawienie stanu pracy dla każdego z projektów.",
) <tabela-stan-pracy>

Repozytorium projektu znajduje się pod adresem: `https://github.com/kamil7430/cil-complexity-analyzer`. Tabela @tabela-stan-repo prezentuje najważniejsze statystyki dotyczące tego repozytorium.

#figure(
  table(
    columns: (auto, 1fr),
    align: horizon,
    stroke: black + 0.5pt,
  
    table.header([*Wskaźnik*], [*Wartość*]),
  
    [Gałąź `main`], [TODO: stan CI],
    [Liczba commitów i scalonych Pull Requestów w okresie], [TODO],
    [Pokrycie kodu testami jednostkowymi (cel NFR-10: $>= 70%$)], [TODO],
    [Liczba testów jednostkowych i integracyjnych], [TODO],
  ),
  caption: "Statystyki repozytorium projektu.",
) <tabela-stan-repo>

= Zrealizowane komponenty <komponenty> // KAMIL dokonał korekty:D (poza działem Kacpra)

== Fasada i generator kodu

Fasada testowa została zaimplementowana zgodnie z projektem z dokumentu etapu 2., bez zmian interfejsu widocznego dla prowadzącego. Zestaw testowy nadal definiuje się jako klasę dziedziczącą po klasie bazowej zestawu, a przypadki testowe jako zagnieżdżone klasy implementujące funkcje `Arrange`, `Act` i `Assert`. Składnia atrybutów konfiguracyjnych, takich jak limit instrukcji CIL, czasu i pamięci, pozostała taka jaką przedstawiono w kodzie źródłowym 1 dokumentu etapu 2. 

Implementacja znajduje się w projekcie `CilComplexityAnalyzer.Facade`, który zawiera atrybuty oraz przyrostowy generator kodu Roslyn. Typy bazowe `TestSuiteBase` i `TestCaseBase` zdefiniowane są w osobnym pakiecie `CilComplexityAnalyzer.TestExecutor.Contract`, dzięki czemu projekty nie odwołują się do siebie nawzajem. 

W trakcie implementacji wprowadzono następujące zmiany wewnętrzne, które nie wpływają na interfejs fasady:

- Wykrywanie zestawów niezależnie od atrybutów konfiguracyjnych. Generator rozpoznaje każdą klasę dziedziczącą po `TestSuiteBase`. Atrybut `IsContainerized` jest wyłącznie metadaną trybu uruchomienia i nie decyduje o generowaniu kodu.

- Przenoszenie pełnej treści klas przypadków testowych. Wszystko, co zawiera ciało klasy `Case` (pola, metody pomocnicze, `Arrange`, `Act`, `Assert`), jest zapisywane jako tekst źródłowy i wstrzykiwane do wygenerowanego pliku, aby kod testu mógł być skompilowany i wykonany przez `TestExecutor`.

== Projekty Contract i TestExecutor

`CilComplexityAnalyzer.TestExecutor.Contract` zawiera wyłącznie typy danych wymieniane między modułami. Nie ma w nim logiki, więc `Fasade`, `TestExecutor` i `ContainerWorker` mogą go referencjonować bez zależności od siebie nawzajem. Dzięki temu nie powstają cykle zależności. `Contract` został zaimplementowany zgodnie z projektem i zawiera następujące typy:

- `TestSuite` opisuje zestaw testowy: nazwę, skompilowane bajty rozwiązania studenta (`StudentSolutionAssemblyBytes`), skompilowane bajty zestawu testów (`TestAssemblyBytes`) oraz listę przypadków testowych (`TestCases`, inicjalizowaną leniwie). Obiekt ten jest budowany na podstawie kodu wygenerowanego przez fasadę i trafia do `TestExecutor`. Kolejne etapy potoku uzupełniają go o swoje wyniki. 
- `TestCase` opisuje pojedynczy przypadek testowy wraz z numerem, który jednoznacznie go identyfikuje.
- `TestSettings` przechowuje konfigurację wynikającą z atrybutów, czyli limity instrukcji CIL, czasu i pamięci oraz tryb uruchomienia (lokalny lub kontener).
- `TestResult` opisuje wynik przypadku testowego: powodzenie lub niepowodzenie z komunikatem, a także zmierzoną liczbę instrukcji CIL. Jest serializowany do formatu JSON przez `ContainerWoker` i odczytywany po stronie `TestExecutor`.

`TestExecutor` jest zaimplementowany jako jedna klasa koordynująca, która nie wykonuje pracy merytorycznej, a jedynie sekwencjonuje etapy potoku i zarządza ich wynikami. Etapy przygotowania kodu to bezstanowe klasy statyczne. Każda przyjmuje obiekt typu `TestSuite` i zwraca go wzbogaconego o kolejne wyniki, więc można je testować w izolacji:

- `StaticAnalyzer` analizuje drzewo składniowe kodu studenta (`AnalyzeStudentSolution`) i kodu testów (`AnalyzeTestSuite`). Odrzuca rozwiązania używające niedozwolonych przestrzeni nazw (`System.IO`, `System.Reflection`, `System.Diagnostics`, `System.Threading`), zanim kod zostanie uruchomiony. Wykrywanie opera się na symbolach, a nie na wyszukiwaniu tekstu. 
- `Compiler` kompiluje rozwiązanie studenta i zestaw testów do pamięci przy użyciu Roslyn (`CompileStudentSolution`, `CompileTestSuite`). Błędy kompilacji są przekazywane studentowi jako czytelny komunikat (US-05), a kod nie jest wtedy uruchomiany. 
- `CilInstructionInjector` wstrzykuje do skompilowanego rozwiązania instrukcje zliczające. Wykorzystuje bibliotekę `CilInjector` opartą o bibliotekę `Mono.Cecil`.

Wykonanie testów wydzielono do klasy `Executor`, która ukrywa za jednym kontraktem dwa tryby uruchomienia: lokalny (w izolowanym `AssemblyLoadContext`) i w kontenerze (`ExecuteInContainer`, przez bibliotekę `TestContainers`). Wyniki są zwracane strumieniowo jako `IEnumerable<TestResult>`, w miarę ukończenia kolejnych testów. 

Od strony użytkownika interfejs `TestExecutor` jest minimalny. Metoda `BeginExecution()` uruchamia potok (jednokrotnie, nawet gdy wywoła ją wiele testów MSTest współbieżnie), a asynchroniczna metoda `GetResult(i)` zwraca wynik i-tego przypadku testowego. Generator Roslyn tworzy wywołania obu metod w klasach MSTest, więc student i prowadzący korzystają z nich tylko przez eksplorator testów w IDE.

Obsługa błędów obejmuje trzy sytuacje:
- Błąd kompilacji lub analizy statycznej. Wszystkie testy zestawu kończą się niepowodzeniem z komunikatem wskazującym miejsce błędu.
- Przekroczenie limitu czasu. Kontener jest zatrzymywany, a testy, które się nie zakończyły, dostają wynik "timeout".
- Nieobsłużony wyjątek. Wszystkie oczekujące testy są oznaczane jako zakończone niepowodzeniem, więc IDE nie czeka bez końca. 

== Izolowane środowisko uruchomieniowe ContainerWorker

Moduł izolowanego środowiska uruchomieniowego jest zaimplementowany jako oddzielny projekt. Składa się z jednej klasy statycznej `Program` -- jest lekkim programem uruchamiającym dostarczoną bibliotekę. Zawiera także statyczną klasę `Paths` z definicjami stałych, zawierającymi ścieżki, pod którymi znajdują się pliki, na których operuje. Jest to ścieżka do pliku `.dll` z zestawem testów oraz ścieżki, pod którą zapisywane są pliki `.json` z wynikami testów.

Proces wykonywania testów wewnątrz klasy `Program` jest podzielony na kilka funkcji, w celu ułatwienia jego jednostkowego testowania.

Uruchomienie programu `ContainerWorker` to wejście do funkcji `Main()`. Jej zadanie sprowadza się do załadowania pliku biblioteki z zestawem testów, a następnie wywołanie funkcji `Execute()`.

Odpowiedzialnością wspomnianej funkcji `Execute()` jest rozpoczęcie procesu wykonywania testów. Zapisuje ona referencję do prawdziwego standardowego wyjścia do zmiennej, a następnie zastępuje używane standardowe wyjście zaślepką, która porzuca wszystko, co jest do niej pisane. Ta operacja wynika bezpośrednio z wymagań klienta, który oczekuje systemu niewrażliwego na wypisywanie na `stdout`. Następnie deleguje pracę do funkcji `FindAllTestCases()` oraz `ActivateAndSortTestCases()`, które zostaną opisane w kolejnych akapitach. Wynikiem ich pracy są gotowe obiekty, na których można przeprowadzać testy. Funkcja `Execute()` przekazuje te gotowe obiekty w poprawnej kolejności do funkcji `PerformSingleTest()`, która wykonuje konkretny test. Po zakończeniu zasadniczej części pracy, następuje powrotne przypisanie prawdziwego standardowego wyjścia w miejsce przypisanej wcześniej zaślepki.

Funkcja `FindAllTestCases()` wyszukuje z wykorzystaniem refleksji wszystkie przypadki testowe w ramach analizowanego zestawu testów.

Funkcja `ActivateAndSortTestCases()` otrzymuje typy przypadków testowych. Jego zadaniem jest stworzenie instancji każdego z otrzymanych typów i ich posortowanie, aby kolejność wykonywania testów odpowiadała kolejności ich zdefiniowania przez prowadzącego.

Zadaniem funkcji `PerformSingleTest()` jest przeprowadzenie konkretnego przypadku testowego. Wyszukuje on refleksją metody `Arrange()`, `Act()`, `Assert()` i wykonuje je w odpowiedniej kolejności. Sprawdza przy tym, czy wykonanie nie kończy się wyjątkiem -- a gdy tak się dzieje, łapie go i zgłasza błąd testu. Na koniec sprawdza, czy wykonanie zmieściło się w limicie operacji CIL i raportuje odpowiedni wynik testu.

Ostatnim elementem tego modułu jest plik `Dockerfile`, zawierający opis sposobu budowy obrazu kontenera Docker. W celu zapewnienia jego minimalnego rozmiaru, proces ten jest podzielony na dwa podetapy:
1. budowa rozwiązania z użyciem SDK .NET 10 (obraz `dotnet/sdk:10.0`) -- jako warstwa `build`,
2. budowa docelowego obrazu kontenera z użyciem lekkiego obrazu `dotnet/runtime:10.0-noble-chiseled` (jako warstwa `final`), który zawiera tylko pliki wymagane przez proces. Artefakty zbudowane w etapie 1 są tutaj kopiowane, a wszystkie pliki pomocnicze są porzucane. Dzięki temu rozmiar wynikowego obrazu jest minimalny i zawiera tylko to, co jest konieczne do jego uruchomienia.

== Biblioteka CilInjection

Moduł `CilInjection` został zaimplementowany jako niezależna biblioteka klas. Jej zadaniem jest instrumentacja (modyfikacja) skompilowanego kodu CIL przekazanego bajtkodu. Biblioteka wymaga aby modyfikowany kod był wywoływany w izolowanym środowisku uruchomieniowym `AssemblyLoadContext`. Biblioteka zawiera przygotowaną architekturę pod implementację nowych sposobów instrumentacji kodu. W dalszej części dokumentu zwane są one strategiami.

Strategie mają (lecz nie muszą) własną dedykowaną bibliotekę (w dalszej części zwane biblioteki Runtime) do której będzie odwoływać się wstrzyknięty przez strategię kod. Biblioteka ta musi zostać załadowana do środowiska uruchomieniowego wraz z zmodyfikowanym kodem. Na obecnym etapie prac zaimplementowano tylko jeden typ bazowy strategii, `BaseCounterStrategy`.

`BaseCounterStrategy` posiada własną bibliotekę Runtime `Counter.Runtime` z klasą  `GlobalCounterContainer`, która zawiera:
- pole Counter typu `long`,
- metodę `GetCounter`, zwracającą wartość pola `Counter`,
- metodę `ResetCounter`, zerującą wartość pola `Counter`.

Klasy bazowe są podstawą pod implementację właściwych strategii wykorzystujących klasę `GlobalCounterContainer`.
Na obecnym etapie prac zaimplementowano i zweryfikowano dwie działające strategie bazujące na `BaseCounterStrat`:
- *`InstructionCounter`:* Strategia zliczająca całkowitą liczbę wykonanych instrukcji CIL podczas działania programu. Mechanizm iteruje po instrukcjach testowanej metody i wplata przed każdą z nich wywołanie zewnętrznej metody inkrementującej globalny licznik.
- *`InstructionCostCounter`:* Strategia zaawansowana, pozwalająca estymować rzeczywisty koszt obliczeniowy uruchomionego kodu. Zamiast prostej inkrementacji o wartość 1, strategia analizuje kod operacji i wstrzykuje instrukcję dodającą do licznika wartość wagową, proporcjonalną do złożoności danej operacji

Cały proces instrumentacji kodu oraz dalsze przygotowanie środowiska uruchomieniowego pod użyte strategie odbywa się za pomocą klasy `CilInjectionManager`. Schemat użycia:
1. Stworzenie wybranych strategii,
2. Stworzenie `CilInjectionManager`,
3. Stworzenie środowiska uruchomieniowego `AssemblyLoadContext`,
4. Dodanie strategii stworzonych w 1. punkcie do `CilInjectionManager`,
5. Wywołanie metody `CilInjectionManager.Transform` podając `AssemblyBytes` kodu, który chcemy zmodyfikować,
6. Wywołanie metody `CilInjectionManager.LoadRuntimeInto` podając stworzony w 3. `AsemblyLoadContext`,
7. Załadowanie wymaganych `Assembly` do `AssemblyLoadContext` stworzonego w 3. 
8. (Opcjonalne) stworzenie uchwytu do metod bilioteki Runtime poprzez wywołanie metody `Strategy.CreateHandle` podając stworzony w 3. punkcie `AsemblyLoadContext`,
9. Wywołanie metody z zmodyfikowanego kodu,
10. (Opcjonalne) użycie metod za pomocą uchwytu stworzonego w 8.

= Instrumentacja bibliotek: ścieżki rozwiązywania // bardziej ej ajowo się nie da

== Problem 
Licznik zlicza tylko te instrukcje, które zostały zmodyfikowane. Kod studenta wykorzystujący biblioteki w praktyce deleguje dużą część pracy do bibliotek: `List<T>`. `Dictionary<TKey, TValue>`, `HashSet<T>` i `Array` znajdują się w `System.Private.CoreLib`, a metody LINQ w `System.Linq`. Jeśli te biblioteki nie są zinstrumentowane, wywołanie `list.Contains(x)` kosztuje niewielką, stałą liczbę instrukcji kodu wołającego, niezależnie od rozmiaru listy. Nie da się wtedy odróżnić algorytmu liniowego od stałego, a to jest sendo oceny złożoności. 

Realizacja tego zadania napotkała dwie przeszkody: // Kamilu uwielbiam Cię za znalezienie tych lsitow '+'
+ *Format R2R*. Biblioteki dostarczane z środowiskiem uruchomieniowym .NET są prekompilowane do formatu ReadyToRun (w skrócie R2R) (_mixed-mode_), czyli zawierają natywny kod maszynowy obok CIL. Przy próbie zapisu zmodyfikowanej biblioteki Mono.Cecil zgłasza wyjątek: `Writing mixed-mode assemblies is not suppored`.
+ *Izolacja kontekstów.* W modelu opartym o `AssemblyLoadContext` każda ładowana biblioteka trafia do osobnego kontekstu i otrzymuje własną kopię biblioteki Runtime, a więc własny licznik. Sumowanie instrukcji z wielu bibliotek wymaga zatem dodatkowych zabiegów.

== Rozpatrzone podejścia <podejscia>

#figure(
  {
    show raw: it => {
      show ".": ".\u{200B}"
      it
    }

    table(
      columns: (32%, 68%), // Lepiej użyć sumy 100% niż 1fr przy sztywnych szerokościach
      align: horizon,
      stroke: black + 0.5pt,

      table.header([*Podejście*], [*Wynik*]),

      [Instrumentacja bibliotek z środowiska uruchomieniowego .NET], [Niemożliwa z powodu R2R (wyjątek Mono.Cecil).],
      [Pakiety NuGet], [Działa dla bibliotek zarządzanych (np. `ASD.Graphs`). Biblioteki `System.*` mają ten sam problem, a typy generyczne leżą w `System.Private.CoreLib`, którego pakiety NuGet nie zawierają.],
      [Zestawy referencyjne (`Microsoft.NETCore.App.Ref`)], [Mono.Cecil potrafi je zmodyfikować i zapisać, ale zawierają tylko sygnatury bez kodu wykonawczego, więc nie można ich użyć do uruchamiania algorytmów.],
      [Estymacja empiryczna bez instrumentacji bibliotek], [Pomiar dla różnych rozmiarów danych (np. $n = 50$ i $n = 200$) i ocena wzrostu (około czterokrotny wskazuje na $O(n^2)$ przy podwojeniu danych, wyraźnie większy na wyższe klasy). Rozwiązanie zapasowe, niewymagające modyfikacji bibliotek.],
      [Ścieżka A: `System.Private.CoreLib` zbudowany ze źródeł (repozytorium open source .NET), ładowany do `AssemblyLoadContext`], [Wersja ze źródeł nie jest w formacie R2R, więc nie występuje problem zapisu. Instrumentacja tej biblioteki powiodła się. Poprawność pomiarów nie została jeszcze zweryfikowana.],
      [Ścieżka B: aplikacja uruchamiana w WebAssembly], [Opisana poniżej. Działa dla wybranych typów kolekcji i LINQ, z deterministycznymi wynikami.],
    )
  },
  caption: "Zestawienie rozpatrzonych sposobów dostępu do kodu bibliotek.",
) <tabela-podejscia>

== Ścieżka A: biblioteki instrumentowane przed uruchomieniem, w kontenerze

Ścieżka ta wykorzystuje bez zmian istniejący mechanizm biblioteki `CilInjection`, czyli klasę `CilInjectionManager` oraz piaskownicę opartą o `AssemblyLoadContext` (klasa `InstrumentedSandbox` pozwalająca wykorzystać szybko bibliotekę `CilInjectora`).

=== Przebieg

+ Pozyskanie wersji `System.Private.CoreLib` w czystym IL. Wersja dostarczana z środowiskiem uruchomieniowym jest w formacie R2R i nie może być zapisana przez Mono.Cecil. Biblioteka została pobrana z projektu open source .NET (GitHub) i nie ma problemu zapisu. 
//TODO: Kamilu w sumie jakbyś tutaj mógł spojrzeć czy to jest ok ten przebieg
+ Instrumentacja każdej biblioteki metodą `CilInjectionManager.Transform`, w jednym przebiegu z kodem studenta, tak aby wszystkie zestawy odwoływały się do tej samej biblioteki Runtime z licznikiem.
+ Utworzenie jednego `AssemblyLoadContext`, załadowanie do niego biblioteki Runtime (`LoadRuntimeInto`), a następnie zinstrumentowanych zależności i zestawu studenta.
+ Uruchomienie testu i odczyt licznika przez uchwyt utworzony dla tego kontekstu (`CreateHandle`).

Ponieważ wszystkie zestawy trafiają do jednego kontekstu i współdzielą jedną kopię biblioteki Runtime, problem osobnych liczników z kilku kontekstów nie występuje.

=== Stan i weryfikacja

Warto tutaj wspomnieć, że obecne rozwiązanie jest implementowane. Nie znamy wszystkich problemów ani wyzwań. Poniższe problemy są tylko problemami, które możemy już teraz stwierdzić. 

+ *Źródło zestawu.* Czy kod wykonywany w piaskownicy używa zinstrumentowanej kopii `System.Private.CoreLib`, a nie zestawu załadowanego przez środowisko uruchomieniowe. Zestaw podstawowy jest ładowany przez środowisko niezależnie, więc kopia w osobnym kontekście jest innym zestawem o innych typach. Sprawdzenie: porównanie kontekstu ładowania i identyfikatora modułu (MVID) typu `List<int>` z bajtami wynikającymi z instrumentacji.
+ *Koszt jednej operacji.* Przy braku instrumentacji bibliotek jedno `list.Add(i)` kosztowało dokładnie cztery instrukcje (liczył się tylko kod wołający). Po instrumentacji powinno kosztować wyraźnie więcej, co jest oczekiwanym wynikiem. Natomiast będzie tutaj wyzwanie wielkości ilości operacji i znalezienia odpowiedniego mechanizmu do testowania poprawności liczenia operacji. Ten problem natomiast odnosi się do obu rozwiązań. 
+ *Skalowanie.* Powtórzenie testów z ścieżki B : wzrost około czterokrotny dla operacji liniowych i około jednokrotny dla operacji o stałym koszcie. ( może znacznie i spokojnie przekroczyć wielkości liczbowe w C\# i może będzie potrzeba napisania własnego typu wielkości liczb)
+ *Utrzymanie*. Zinstrumentowane biblioteki są częścią obrazu kontenera, więc ich aktualizacja wymaga ponownego zbudowania obrazu albo przynajmniej podmianę bibliotek na nowo zinstrumentowane.


Jednak prawdopodobnie będzie to prostsze rozwiązanie niż wykorzystanie WebAssembly.

== Ścieżka B: WebAssembly <sciezka-wasm>

=== Idea

W pakiecie aplikacji WebAssembly (platforma `wasi-wasm`, .NET 10) biblioteki systemowe są dostarczane jako czysty IL, bez kodu natywnego, więc Mono.Cecil może je zapisać. Zamiast uruchamiać kod studenta w standardowym środowisku .NET, kompilujemy aplikację do pakietu `AppBundle`, modyfikujemy znajdujące się w nim biblioteki i uruchamiamy całość w środowisku `wasmtime` (interpreter Mono wewnątrz `dotnet.wasm`). Cały proces ma jeden wspólny licznik, więc problem izolacji kontekstów nie występuje.

=== Przebieg testu 

#figure(
  ```text
  HOST (.NET)                              GOSC (wasmtime, Mono w WASM)
  -----------------------------            -----------------------------
  1. kopia AppBundle do katalogu
     tymczasowego
  2. instrumentacja StudentWasmApp.dll
  3. instrumentacja bibliotek
     (CoreLib, System.Collections,
     System.Linq), z pamięcią podręczną
  4. uruchomienie wasmtime  ---------->    Program.Main
                                            - znalezienie licznika
                                            - rozgrzewka
                                            - Reset, algorytm, Get
  5. odczyt standardowego wyjścia <-----    wypisanie wyniku
  6. asercje i sprzątanie
  ```,
  caption: "Przebieg testu instrumentacji w WebAssembly.",
) <rysunek-wasm>


Wynik mierzonego algorytmu jest przekazywany przez standardowe wyjście w postaci linii `[INSTRUCTION_COUNTER_RESULT]=...`, którą host odczytuje po zakończeniu procesu. Testy pracują na kopii pakietu, dzięki czemu oryginał nie jest nadpisywany, a instrumentacja nie jest wykonywana dwukrotnie. Instrumentowane biblioteki są zapisywane w pamięci podręcznej, której klucz zawiera skrót oryginalnego pliku, identyfikator zbudowanego wstrzykiwacza i parametry instrumentacji.

=== Chronologia prób <chronologia-wasm>

+ *Licznik w aplikacji, selektywna instrumentacja `System.Private.CoreLib`.* Licznik (pole statyczne w klasie `<GlobalCounterContainer>`) znajdował się w bibliotece aplikacji. Biblioteki systemowe odwoływały się do niego przez referencję. Instrumentacja całej biblioteki `System.Private.CoreLib` (ok. 12 MB) kończyła się błędem przepełnienia stosu wywołań (`wasm trap: call stack exhausted`). Ograniczenie instrumentacji do kilku typów kolekcji nie pomogło.
+ *Przyczyna: cykl zależności.* `StudentWasmApp.dll` używa `List<T>` z `System.Private.CoreLib`, a zinstrumentowany `List<T>` odwołuje się do licznika w `StudentWasmApp.dll`. Przy pierwszym wykonaniu zinstrumentowanej metody interpreter Mono rozwiązuje token pola z innego zestawu (`interp_field_from_token`), co wymaga wykonania zarządzanego kodu `System.Private.CoreLib`. Ten kod również ma wstrzyknięty licznik, więc próbuje rozwiązać ten sam token, i powstaje nieskończona rekurencja jeszcze przed wykonaniem pierwszej instrukcji aplikacji.
+ *Rozwiązanie: licznik w `System.Private.CoreLib`.* Klasa `CilCounter.GlobalCounter` (pole `Count` typu `long` oraz metody `GetInstructionCount` i `ResetInstructionCount`) jest definiowana wewnątrz `System.Private.CoreLib`, a pozostałe zestawy (aplikacja, `System.Collections`, `System.Linq`, biblioteki zewnętrzne) jedynie się do niej odwołują. Każdy zestaw i tak zależy od `System.Private.CoreLib`, więc cykl nie występuje. W samym `System.Private.CoreLib` pole jest zwykłą definicją z tego samego modułu, więc jego rozwiązanie nie wymaga ładowania innego zestawu. Klasa nie ma konstruktora statycznego, więc dostęp do pola nie uruchamia żadnego kodu inicjalizującego. Aplikacja odczytuje i zeruje licznik przez delegaty tworzone jednokrotnie (nie przez `MethodInfo.Invoke`), aby refleksja nie doliczała instrukcji do wyniku.
+ *Poprawki implementacji.* W trakcie prac wykryto i poprawiono błędy zestawione w tabelce @tabela-bledy.


#figure(
  table(
    columns: (55%, 1fr),
    align: horizon,
    stroke: black + 0.5pt,

    table.header([*Problem w pierwotnej implementacji*], [*Skutek*]),

    [Filtr pomijał typy o nazwach zaczynających się od `<` (`<>c`, `<>c__DisplayClass`, `<Foo>d__N`)], [Ciała lambd i iteratorów nie były liczone.],
    [Filtr LINQ sprawdzał nazwę typu zawierającą `Enumerable` także dla typów zagnieżdżonych], [Iteratory LINQ nie były liczone.],
    [Typy startowe wyszukiwane przez `StartsWith("List")`], [Trafiano w przypadkowy typ.],
    [Przestrzeń nazw sprawdzana po dodaniu typu do zbioru odwiedzonych], [Filtr nie filtrował.],
    [Globalny, statyczny zbiór dozwolonych typów], [Wynik zależał od kolejności instrumentacji zestawów.],
    [Prefiksy CIL (`constrained.`, `volatile.`) mapowane na samych siebie], [Skoki do prefiksu omijały licznik.],
    [Brak zwiększenia `MaxStackSize`], [Wstawka wymaga dwóch dodatkowych slotów stosu.],
  ),
  caption: "Błędy wykryte i poprawione w implementacji wstrzykiwacza.",
) <tabela-bledy>

#figure(
  table(
    columns: (1fr,1fr, 1fr),
    align: horizon,
    stroke: black + 0.5pt,

    table.header([*Cecha*], [*Ścieżka A*], [*Ścieżka B*]),

    [Środowisko], [Kontener `ContainerWorker` (bez zmian względem etapu 2)], [`wasmtime` z interpreterem Mono  (dodatkowo do zrobienia osadzenie w kontenerze)],
    [Źródło `System.Private.CoreLib`], [Wersja z projektu open source .NET], [Wersja z pakietu aplikacji WebAssembly (czysty IL)],
    [Miejsce licznika], [Biblioteka Runtime w `CilInjection`], [Klasa w `System.Private.CoreLib`],
    [Instrumentacja bibliotek], [Przed operacją liczenia, biblioteki w obrazie], [Przed operacją liczenia, biblioteki w pakiecie `AppBundle`],
    [Stan], [Instrumentacja udana, pomiary niezweryfikowane], [Zweryfikowana dla kolekcji i LINQ.],
    [Główna niewiadoma], [Odwołanie z `System.Private.CoreLib` do osobnej biblioteki Runtime], [Zakres typów dobierany z góry albo dodatkowy sposób dobierania typów/modułów z góry],
  ),
  caption: "Porównanie ścieżek instrumentacji bibliotek.",
) <tabela-porownanie>


= Pierwszy działający test end-to-end <1st-e2e> // kamilos

W tym rozdziale zaprezentowane zostanie działanie systemu w stanie na moment redagowania niniejszego dokumentu (dokumentu 3 etapu). 

Jak wspomniano w poprzednich rozdziałach, niektóre moduły są już gotowe. W związku z tym, możliwe jest pierwsze wstępne uruchomienie systemu end-to-end. Obejmuje ono:
- zdefiniowanie zestawu testów z wykorzystaniem fasady testowej i generatora kodu testów, 
- pełne przejście przez potok modułu TestExecutor (z wyłączeniem wstrzykiwania kodu CIL),
- uruchomienie i wykonanie testów w kontenerze Docker,
- przekazanie wyników do menedżera testów w IDE.

Pierwszym krokiem, który należy wykonać, aby móc uruchomić prezentowany system, jest zdefiniowanie zestawu testów. Kod źródłowy @e2e-przykladowy-kod prezentuje przykładową klasę `TestSuite`, która zawiera proste instrukcje do wykonania w ramach testów. Należy tu zaznaczyć, że wewnątrz funkcji `Arrange()`, `Act()` oraz `Assert()` mogą znajdować się dowolnie skomplikowane ciągi instrukcji. Jedynym ograniczeniem jest zbiór bibliotek, do których ma dostęp kompilator wewnątrz modułu `TestExecutor` oraz izolowane środowisko uruchomieniowe.

#figure(
  supplement: "Kod źródłowy",
  caption: [Kod zestawu testów, który został uruchomiony w ramach Rozdziału @1st-e2e.],
)[
```cs
[TestSuite]
[Containerized]
[StudentSolution(typeof(Student1Solution))]
public partial class Lab01Evaluation : TestSuite
{
    [TestCase]
    public class Case1 : TestCase
    {
        private int _liczba = 3;
        public override int TestNumber() => 0;
        public override void Arrange() { _liczba = 5; }
        public override void Act() { _liczba *= _liczba; }
        public override void Assert() { IsTrue(_liczba > 0); IsTrue(_liczba == 25); }
    }

    [TestCase]
    public class Case2 : TestCase
    {
        public override int TestNumber() => 1;
        public override void Arrange() { }
        public override void Act() { }
        public override void Assert() { IsTrue(0 < 1); IsTrue(1 < 1); IsTrue(2 < 1); }
    }
}
```
] <e2e-przykladowy-kod>

Klasa zestawu testów zaprezentowana w Kodzie źródłowym @e2e-przykladowy-kod zostaje następnie rozpoznana przez IDE. Na Rysunku @e2e-wykryte-testy widoczne są rozpoznane dwie klasy zestawów testowych (`Lab01Evaluation_Tests` oraz `Lab02Evaluation_Tests`). Pierwsza z nich to wynik działania generatora kodu opartego na Kodzie źródłowym @e2e-przykladowy-kod.

Z poziomu tego widoku jest możliwe uruchomienie testów jednym kliknięciem -- zarówno konteneryzowanych, jak i niekonteneryzowanych. Możliwe jest również uruchomienie debuggera, który podczas zajęć pozwala przyjrzeć się działaniu algorytmu na konkretnych danych testowych.

#figure(
  image("e2e-okienko-testow-wykryte.png"),
  supplement: "Rysunek",
  caption: [Widok menedżera testów w IDE JetBrains Rider. Zawiera on dwa zestawy testów -- `Lab01Evaluation_Tests` oraz `Lab02Evaluation_Tests`.],
) <e2e-wykryte-testy>

Po uruchomieniu zestawu testów za pomocą odpowiedniego przycisku w menedżerze testów IDE następuje rozpoczęcie pracy systemu. Opisane w poprzednich rozdziałach moduły kolejno wykonują swoją pracę. 

Przykładowym dowodem na funkcjonowanie systemu może być widoczny na Rysunku @e2e-docker-desktop-zrzut zrzut ekranu z podglądu kontenera Docker w programie Docker Desktop. Widzimy na nim logi izolowanego środowiska uruchomieniowego `ContainerWorker`, uruchomionego w trybie debugowania, które informują nas o:
- załadowaniu biblioteki `.dll` z zestawem testów,
- załadowaniu z zasobów tej biblioteki rozwiązania studenta,
- odnalezieniu dwóch przypadków testowych (klas dziedziczących po `TestCase`),
- stworzeniu instancji każdego z testów,
- wywołaniu `Arrange()`, `Act()`, `Assert()` dla pierwszego z testów oraz zapisaniu jego pomyślnego zakończenia do pliku JSON,
- oraz wywołaniu tych metod dla drugiego z testów, tym razem zakończonego niepowodzeniem (`IsTrue(1 < 1)`) i zapisania tego wyniku do pliku JSON.

Po zakończeniu wykonywania testów w kontenerze, zostaje on usunięty.

#figure(
  image("e2e-docker-desktop.png"),
  supplement: "Rysunek",
  caption: [Podgląd logów programu `ContainerWorker` uruchomionego wewnątrz kontenera dla zestawu testowego zdefiniowanego \ w Kodzie źródłowym @e2e-przykladowy-kod.]
) <e2e-docker-desktop-zrzut>

Po zakończeniu pracy przez `ContainerWorker`, wyniki są odczytywane z kontenera i zwracane przez moduł `TestExecutor`. Po zakończeniu tego procesu ostateczne wyniki są widoczne w menedżerze testów w IDE, co pokazuje Rysunek @e2e-okienko-testow-wyniki.

#figure(
  image("e2e-okienko-testow.png"),
  supplement: "Rysunek",
  caption: [Widok menedżera  testów w IDE JetBrains Rider po zakończeniu wykonywania zestawu testów `Lab01Evaluation_Tests`.],
) <e2e-okienko-testow-wyniki>

W przypadku wersji uruchamianej w ramach CI/CD w repozytoriach na zajęciach ASD2, wyniki będą zwracane w przystępnej formie tekstowej, np. w formacie JSON. Dzięki temu będą mogły być dalej przetwarzane i wykorzystywane, również automatycznie.


= Odstępstwa od projektu z etapu 2 <odstepstwa>

#table(
  columns: (auto, 1fr, 1fr),
  align: horizon,
  stroke: black + 0.5pt,

  table.header([*Obszar*], [*Etap 2*], [*Etap 3*]),

  [Środowisko wykonawcze], [Kontener Docker uruchamiający bibliotekę `.dll`.], [TODO: relacja kontenera Docker do `wasmtime` w ścieżce B.],
  [Licznik instrukcji], [Osobna biblioteka z polem `Counter`.], [W ścieżce B licznik w `System.Private.CoreLib`.],
  [Analiza statyczna], [Blokada m.in. `System.IO`, `System.Reflection`, `System.Diagnostics`, `System.Threading`.], [Testy instrumentacji wykonywano także na tych przestrzeniach nazw; blokada obowiązuje w kodzie studenta, nie w bibliotekach.],
  [Wiele rozwiązań], [Obsługa listy rozwiązań.], [Usunięto `SolutionList`: testy działają dla jednego studenta.],
)


= Problemy i ryzyka <ryzyka>

- Instrumentacja bibliotek (protokół awaryjny 2 z etapu 1): plan A (przechwytywanie bibliotek w czasie wykonania) zawiódł z powodu R2R. Aktualnie realizowane są plany B i WebAssembly, TODO: ocena, który wariant z protokołu odpowiada ścieżce A.
- Narzut czasowy instrumentacji (protokół awaryjny 3): TODO pomiar względem NFR-03 (≤ 5×) i NFR-04 (≤ 30 s).
- Aktualizacja rejestru ryzyk RSK-01...05: TODO.

= Zgodność z harmonogramem i prognoza

#table(
  columns: (auto, auto, 1fr),
  align: horizon,
  stroke: black + 0.5pt,

  table.header([*Zadanie (W06-W08)*], [*Status*], [*Uwagi*]),

  [Silnik analizy złożoności], [TODO %], [Moduł `CilInjector` rozszerzony o `LibraryCilInjector`.],
  [Moduły uruchamiania testów], [TODO %], [Zaimplementowane.],
  [Pierwszy przykład end-to-end], [TODO], [TODO],
)

Prognozowany termin ukończenia: TODO. Plan na etap 4 (W09-W10): integracja fasady z modułami, konfiguracja GitHub Actions, domknięcie wybranej ścieżki instrumentacji bibliotek.
