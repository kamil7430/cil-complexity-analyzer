// #import "@preview/ilm:2.1.1": *

// #show: ilm.with(
//   title: [Dokument etapu 2: \ _Architektura systemu i projekt techniczny_],
//   authors: "Projekt inżynierski: Projekt i implementacja platformy testowej weryfikującej i profilującej rozwiązania programistyczne.",
//   abstract: [],
//   figure-index: (enabled: false),
//   table-index: (enabled: false),
//   listing-index: (enabled: false),
//   table-of-contents: none,
// )

#import "@preview/rubber-article:0.5.2": *

#show: article

#set text(lang: "pl")

#set page(numbering: none)

#align(horizon)[
  #maketitle(
    title: [Dokument etapu 2: _Architektura systemu i \ projekt techniczny_],
    authors: ([Tytuł projektu: _Projekt i implementacja platformy testowej weryfikującej i profilującej rozwiązania programistyczne_ \ \ Autorzy: Kacper Grobel, Kamil Błażejczyk, Kornel Dzieża \ Promotorzy: mgr inż. Tomasz Herman, dr inż. Michał Dębski],),
  )

  #align(bottom)[
    #image("logo-mini.png")
    #align(center)[4 listopada 2026]
  ]
]
  
#pagebreak()

#counter(page).update(1)
#set page(numbering: "1")
#set ref(supplement: none)

//#heading(outlined: false, numbering: none)[Dokument etapu 2: Architektura systemu i projekt techniczny]

// *Projekt inżynierski -- Wydział Matematyki i Nauk Informacyjnych, Politechnika Warszawska*

// *Tytuł projektu*: _Projekt i implementacja platformy testowej weryfikującej i profilującej rozwiązania programistyczne._

// *Autorzy:*

// - *Kacper Grobel* (KG) -- `kacper.grobel.stud@pw.edu.pl`
// - *Kamil Błażejczyk* (KB) -- `kamil.blazejczyk.stud@pw.edu.pl`
// - *Kornel Dzieża* (KD) -- `kornel.dzieza.stud@pw.edu.pl`

// *Promotorzy:*

// - *mgr inż. Tomasz Herman* -- `tomasz.herman@pw.edu.pl`
// - *dr inż. Michał Dębski* -- `michal.debski@pw.edu.pl`

#heading(depth: 2, numbering: none, outlined: false)[Abstrakt]

Niniejszy dokument przedstawia architekturę platformy testowej przeznaczonej do zautomatyzowanej weryfikacji i profilowania studenckich rozwiązań programistycznych. Głównym celem opracowania jest zdefiniowanie struktury modułowej powstającego rozwiązania, określenie odpowiedzialności poszczególnych komponentów oraz opisanie interfejsów i warstw komunikacyjnych pomiędzy nimi. Ponadto dokument zawiera graficzne schematy przepływu sterowania, wizję punktów styku z użytkownikiem oraz merytoryczne uzasadnienie wyboru stosu technologicznego.

#heading(depth: 2, numbering: none, outlined: false)[Historia zmian dokumentu]

#table(
  columns: (10%, 12.5%, 17.5%, 60%),
  align: center,
  stroke: black + 0.5pt,
    
  table.header([*Wersja*], [*Data*], [*Autorzy*], [*Opis zmian*]),
  
  [1.0], [04.11.2026], [KG / KB / KD], [Wersja bazowa dokumentu],
  [1.1], [04.11.2026], [KG], [Zmiana opisu mechanizmu komunikacji i przepływu danych pomiędzy modułem CIL Injection a Test Executorem],
)

#heading(depth: 2, numbering: none, outlined: false)[Spis treści]

#outline(title: none)

#pagebreak()

#heading(depth: 2, numbering: none, outlined: false)[Słowniczek pojęć]

#show figure: set block(breakable: true)

#figure(
  table(
    columns: (auto, 70%),
    align: horizon,
    stroke: black + 0.5pt,
    
    table.header(
      [*Pojęcie*],
      [*Definicja*],
    ),
    
    [AAA (_Arrange-Act-Assert_)], [Wzorzec projektowania testów dzielący je na trzy fazy: przygotowanie środowiska (Arrange), wykonanie akcji (Act) oraz weryfikację wyników (Assert).],
    [Analiza statyczna], [Badanie kodu źródłowego (np. jego drzewa składniowego) przed uruchomieniem, używane w systemie m.in. do weryfikacji użycia niedozwolonych konstrukcji.],
    [ASD2 (_Algorytmy i Struktury Danych 2_)], [Przedmiot, dla którego jest przeznaczony ten projekt i na którego potrzeby powstaje platforma.],
    [Asercja (_Assert_)], [Instrukcja w teście weryfikująca, czy uzyskany wynik lub stan (np. spójność grafu) jest zgodny z oczekiwaniami.],
    [Assembly / Assembly Bytes], [Skompilowany plik biblioteki (np. `.dll`) lub jego reprezentacja w pamięci w postaci tablicy bajtów, poddawana modyfikacjom i uruchamiana w kontenerze.],
    [C\#], [Obiektowy język programowania ogólnego przeznaczenia kompilowany do kodu CIL. Język, w którym studenci piszą rozwiązania.],
    [CI/CD (_Continuous Integration / Deployment_)], [Praktyka automatyzacji budowania, testowania i wdrażania oprogramowania przy każdej zmianie kodu. Środowisko wykonujące te czynności nazywa się środowiskiem CI/CD.],
    [CIL (_Common Intermediate Language_)], [Język pośredni, do którego kompiluje się kod C\# przed ostatecznym wykonaniem przez środowisko uruchomieniowe .NET.],
    [CLI (_Command Line Interface_)], [Interfejs tekstowy, w którym programem steruje się i w którym prezentowane są wyniki w postaci logów lub tabel.],
    [Docker], [Narzędzie do konteneryzacji aplikacji, pozwalające na tworzenie powtarzalnych i bezpiecznych środowisk.],
    [Drzewo składniowe (_Syntax Tree_)], [Hierarchiczna struktura danych reprezentująca kod źródłowy programu po jego przetworzeniu przez kompilator.],
    [Fasada (Fasada testowa)], [Wzorzec projektowy ukrywający skomplikowaną logikę uruchamiania testów za prostym w użyciu interfejsem deklaratywnym przeznaczonym dla prowadzącego.],
    [GitHub Actions], [System CI/CD serwisu GitHub, w którym środowisko i procesy automatycznej weryfikacji są zdefiniowane jako tzw. akcje.],
    [IDE (_Integrated Development Environment_)], [Zintegrowane środowisko programistyczne (np. JetBrains Rider, Visual Studio) służące do tworzenia i lokalnego testowania kodu.],
    [Kontener / konteneryzacja], [Kontener to izolowane środowisko uruchomieniowe aplikacji współdzielące jądro systemu gospodarza, ale blokujące dostęp do jego zasobów (np. sieci). Konteneryzacja to proces używania takich środowisk.],
    [Model semantyczny], [Struktura w kompilatorze Roslyn dostarczająca informacji o znaczeniu elementów kodu, wykorzystywana do odczytywania atrybutów i typów.],
    [MSTest], [Wbudowany w platformę Microsoft framework do testów jednostkowych, którego mechanizmy są tu użyte do integracji wyników weryfikacji z interfejsem narzędzi w IDE.],
    [.NET 10], [Wersja platformy programistycznej firmy Microsoft. W jej skład wchodzi środowisko uruchomieniowe wykonujące kod CIL i izolujące zarządzanie pamięcią.],
    [NuGet], [Narzędzie oraz ekosystem zarządzania pakietami (bibliotekami) dla platformy .NET, służące jako metoda dystrybucji systemu do użytkowników.],
    [Orkiestracja / Orkiestrator], [Automatyczne zarządzanie, układanie w sekwencję i koordynowanie zadań pomiędzy niezależnymi modułami (np. zadanie modułu Test Executor).],
    [Przestrzeń nazw (_Namespace_)], [Mechanizm C\# grupujący typy i organizujący ich nazwy (np. `System.IO`).],
    [Refleksja (_Reflection_)], [Mechanizm języka C\# pozwalający programowi na dynamiczne skanowanie swoich struktur (np. wyszukiwanie klas testowych lub metod) w trakcie działania aplikacji.],
    [Roslyn], [Platforma kompilatora .NET używana w systemie do analizy statycznej, dynamicznej kompilacji w pamięci oraz przyrostowego generowania kodu w locie.],
    [Sprawdzarka], [Potoczne określenie zautomatyzowanego systemu oceniającego rozwiązania studentów.],
    [stdout (_Standard Output_)], [Standardowy strumień wyjścia procesu, na który system wypisuje rezultaty testów w potoku automatyzacji procesów (CI/CD).],
    [Test Executor], [Komponent orkiestrujący: analizuje kod, deleguje wplatanie instrukcji CIL, osadza pliki w kontenerze, uruchamia proces i zwraca wyniki.],
    [Wstrzykiwanie / Instrumentacja CIL], [Modyfikacja skompilowanego zestawu przez dodanie instrukcji języka pośredniego (np. liczników operacji) bez konieczności jakiejkolwiek ingerencji w kod źródłowy.]
  ),
)

// dwa ostatnie potrzebne do rys.3 

= Projekt i architektura systemu

Niniejszy rozdział szczegółowo przedstawia architekturę oraz projekt techniczny zautomatyzowanej platformy testowej. Prezentuje on ogólny podział systemu na niezależne, lecz współpracujące ze sobą moduły, jednocześnie określając ich granice i obowiązki podczas weryfikacji oraz profilowania kodu. W kolejnych podrozdziałach omówiono projekt interfejsów, warstwę spajającą poszczególne komponenty, a także  diagramy modelujące przepływ sterowania i cykle życia kluczowych obiektów. Dopełnieniem opisu architektonicznego jest wizja punktów styku użytkownika z platformą (zarówno w środowiskach IDE, jak i potokach CI/CD). 

Ogólny zarys zaprojektowanej architektury, obrazujący hierarchię oraz relacje pomiędzy najważniejszymi elementami rozwiązania, został zilustrowany na rysunku @moduly-schemat.

#figure(
image("obraz (4).png"),
  supplement: "Rysunek",
  caption: [Diagram modułów architektury systemu.]
) <moduly-schemat>



== Architektura systemu -- moduły i ich odpowiedzialności

Architektura platformy została zaprojektowana w oparciu o podział na wyspecjalizowane, słabo powiązane moduły. Każdy z nich realizuje ściśle określony zakres odpowiedzialności.
Taki podział gwarantuje wysoką modularność, ułatwia utrzymanie i rozbudowę systemu oraz zapewnia bezpieczne środowisko do weryfikacji rozwiązań programistycznych dostarczanych przez studentów. 
Główny przepływ sterowania oraz obieg danych w systemie opiera się na współpracy trzech kluczowych komponentów:
1. Moduł koordynacji testów (Test Executor) -- stanowiącego centralny punkt orkiestracji procesu testowego.
2. Moduł modyfikacji kodu pośredniego (CilInjection) -- odpowiedzialnego za niskopoziomową instrumentację kodu CIL i zliczanie instrukcji.
3. Izolowanego środowiska uruchomieniowego -- zapewniającego bezpieczną i odseparowaną egzekucję testów wewnątrz kontenerów. 
// (w jednym słowie: jest spełniona zasada Single responsibility)

=== Izolowane środowisko uruchomieniowe <container-worker-module>

Jednym z podstawowych wymagań biznesowych dla nowego systemu jest izolacja rozwiązań studentów. Ma ona na celu zarówno wykluczenie możliwości wyrządzenia szkód przez złośliwy kod, który może być wysłany do systemu, jak i zapewnienie czystego środowiska dla każdej kolejnej instancji testującej kody studentów.

Komponentem realizującym to wymaganie jest izolowane środowisko uruchomieniowe, czyli konteneryzowany program uruchamiający przygotowany wcześniej zestaw testów. Rozwiązanie to ogranicza możliwość oddziaływania wykonywanemu kodowi z systemem, który je uruchamia, a ponadto daje pełną kontrolę nad działającym środowiskiem. W przypadku, gdy program testujący zawiesi się albo wpadnie w nieskończoną pętlę (np. z powodu błędu logicznego w kodzie studenta), cały kontener można wyłączyć ("zabić"), po prostu wykonując komendę `docker kill`. Co więcej, takie wyłączanie można powiązać z czasomierzem i ustalić czas, w jakim wykonanie powinno się zakończyć, a gdy tak się nie stanie, natychmiast wyłączyć kontener.

Zakres odpowiedzialności tego modułu jest następujący:
- wczytanie otrzymanego zestawu testów,
- przeprowadzenie testów,
- zapisanie wyniku każdego przeprowadzonego testu do pliku.

=== Moduł modyfikacji kodu pośredniego CIL (biblioteka CilInjection) <libka-cil-injection>

Całkowicie wewnętrzny komponent systemu, wydzielony w postaci niezależnej biblioteki, odpowiadający za niskopoziomową manipulację skompilowanym kodem CIL. Moduł ten jest całkowicie ukryty przed użytkownikami końcowymi (zarówno autorami testów, jak i studentami). Jego wyłącznym konsumentem jest wewnętrzny mechanizm frameworku. Został on wyizolowany w celu zapewnienia czystości architektury oraz ułatwienia utrzymania projektu.

Zadania modułu podzielone są na dwa wewnętrzne obszary funkcjonalne:

- Definiowanie reguł i rozbudowa systemu (dla deweloperów utrzymujących framework):
  - Udostępnianie ustandaryzowanych struktur i klas bazowych, pozwalających programistom na łatwe dodawanie nowych, wewnętrznych mechanizmów transformacji kodu IL, bez konieczności ingerencji w główny kod silnika wykonawczego.
  - Dostarczanie abstrakcji ukrywających złożoność manipulacji surowymi instrukcjami języka pośredniego.

- Mechanizm wykonawczy (wykorzystywany przez silnik frameworku):
  - Zarządzanie potokiem modyfikacji -- przyjmowanie skompilowanych plików (_Assembly Bytes_) testowanego kodu, sekwencyjne aplikowanie wymaganych transformacji i generowanie zmodyfikowanych plików (_Assembly Bytes_).
  - Zapewnienie mechanizmów do bezpiecznego, dynamicznego ładowania zmodyfikowanego kodu w izolowanym kontekście pamięci (`AssemblyLoadContext`).
  - Zapewnienie mechanizmu do bezpiecznego, dynamicznego ładowania wymaganych przez wprowadzone modyfikacje wewnętrznych bibliotek do izolowanego kontekstu pamięci (`AssemblyLoadContext`).
  - wygenerowanie wymaganych przez zmiany w kodzie studenta bibliotek pomocniczych (_Assembly Bytes_).
  - Zapewnienie pełnych ścieżek do metod do obsługi bibliotek pomocniczych, w celu umożliwienia późniejszego ich wywołania za pomocą refleksji.
  
=== Moduł koordynacji testów (Test Executor) <test-executor-module>

Najważniejszym komponentem łączącym wszystkie pozostałe w całość jest moduł koordynatora. Odpowiada on za kompleksowe przeprowadzenie testów rozwiązania studenta. W trakcie swojej pracy korzysta on ze wszystkich wyżej opisanych komponentów, przygotowując im dane do przekazania, odbierając wyniki ich pracy i przekazując je dalej. Właśnie ten moduł definiuje kontrakt, za pomocą którego odbywa się komunikacja z użytkownikiem systemu. 

Poza odgrywaniem funkcji delegującego zadania, moduł Test Executor odpowiada także za pomniejsze zadania, które nie są na tyle znaczące, żeby były wydzielone do osobnego modułu. Takimi zadaniami są:
- analiza statyczna kodu (wstępny przegląd kodu, który np. może odrzucić rozwiązanie na podstawie użycia niedozwolonej przestrzeni nazw),
- kompilacja kodu.

Dokładny zakres odpowiedzialności koordynatora wynika z kształtu procesu uruchamiania testów i zwracania wyników do użytkownika i wygląda następująco:
- interpretacja testów zdefiniowanych przez użytkownika,
- zarządzanie testami w sposób przyjazny dla IDE (obsługa współbieżności),
- uruchomienie i obsługa potoku testowego:
  - analiza statyczna,
  - dynamiczna kompilacja do pamięci,
  - wstrzyknięcie kodu CIL zliczającego instrukcje (z wykorzystaniem biblioteki opisanej w rozdziale @libka-cil-injection),
  - przygotowanie środowiska opisanego w rozdziale @container-worker-module i uruchomienie w nim testów rozwiązania studenta,
  - oczekiwanie na wyniki spływające z wyżej wymienionego środowiska z uwzględnieniem możliwości wystąpienia niepowodzeń,
  - przekazanie wyników do użytkownika.
  
== Projekt modułów --  interfejsy i zależności <modules-design-interfaces-dependencies-layers>

// Cel: Pokazanie, jak moduły komunikują się ze sobą w kodzie i kto od kogo zależy.

W tym rozdziale dokładniej opisane zostaną kontrakty i hierarchia zależności pomiędzy zaprezentowanymi wcześniej modułami. Opisany zostanie także graf zależności, widoczny na rysunku @moduly-schemat.

Jak wspomniano w rozdziale @test-executor-module, moduł koordynacji testów jest elementem, który łączy pozostałe komponenty w całość. 
// Zbiera on szczegóły testów od ich twórcy (prowadzącego zajęcia) w jedną strukturę. Na etapie przygotowania do uruchomienia testów do struktury tej dopisywane są wyniki kolejnych wykonywanych operacji -- drzewo składniowe (wynik działania analizy statycznej) czy tablica bajtów (wynik kompilacji).
Z tego powodu jako wejściową część jego kontraktu należy przyjąć strukturę otrzymywaną poprzez fasadę, która zostanie kompleksowo opisana w rozdziale @fasada. Jako wyjście, moduł ten zwraca wyniki testów, do których można się dostać poprzez współbieżnie bezpieczną funkcję (_getter_). Z punktu widzenia użytkownika końcowego, funkcja ta będzie wywoływana asynchronicznie przez kod automatycznie wygenerowany na podstawie zdefiniowanego wcześniej zestawu testów. W ten sposób, każda interakcja z systemem możliwa będzie za pośrednictwem narzędzia uruchamiania testów w ramach użytkowanego IDE.

Podczas swojej pracy, moduł koordynujący będzie zależny zarówno od biblioteki wstrzykującej instrukcje CIL, jak i izolowanego środowiska uruchomieniowego, które są wyspecjalizowanymi komponentami realizującymi konkretne zadania. Z tego powodu w obu przypadkach kontrakt jest minimalny.

Moduł wstrzykujący kod CIL jako wejście otrzymuje kod studenta jako skompilowany plik (_Assembly Bytes_). Skompilowany kod studenta jest zmieniany poprzez dodanie sekwencji instrukcji inkrementujących licznik. Sekwencja ta jest dodawana między każdą instrukcję CIL. Licznik znajduje się w osobnej bibliotece zawierającej pole Counter.
Wyjściem tego modułu są nowy skompilowany plik kodu studenta (_Assembly Bytes_), skompilowany plik biblioteki z polem Counter (_Assembly Bytes_) oraz pełne ścieżki do metod z biblioteki z polem Counter. Moduł ten jest zawsze uruchamiany przez Test Executor.

Izolowane środowisko uruchomieniowe jako wejście otrzymuje gotową do uruchomienia bibliotekę (`.dll`), zawierającą zestaw testów. Każdy z testów jest wykrywany za pomocą refleksji i uruchamiany sekwencyjnie. bibliotekę (`.dll`) zawierającą kod studenta, bibliotekę (`.dll`) z polem Counter oraz pełne ścieżki do metod z biblioteki z polem Counter. Metody do biblioteki z polem Counter są wykrywane poprzez refleksje i wywoływane w celu dostania lub zresetowania licznika. Wyjściem tego modułu jest zestaw wyników wszystkich testów, zapisywany do plików. W kontekście całego systemu, moduł ten zawsze jest uruchamiany przez Test Executor, który ma również możliwość przeglądania plików wewnątrz kontenera. Z tego powodu ma on dostęp do informacji o wynikach testów na bieżąco, gdyż sprawdzenie tego faktu to wyszukanie odpowiedniego pliku wewnątrz kontenera. Schemat komunikacji pomiędzy izolowanym środowiskiem uruchomieniowym a modułem koordynującym zostanie dokładniej opisany w rozdziale @communication-layer-design-protocols-libraries-network.

== Projekt warstwy komunikacyjnej <communication-layer-design-protocols-libraries-network>

// między test executorem a IŚU mamy komunikację plikową
// w pozostałych przypadkach jest to po prostu wewnątrzprocesowa komunikacja
// ci w repo studenta

// 1. Po "spuszowaniu" do repozytorium kodu studenta trafia on do Test executora
// 2. Test executor przyjmuje kod studenta i przekazuje go już komunikacją wewnątrzprocesową do CilInjectora 
// 3. Zinstrumentowane bajty są przekazane do IUŚ i odpalone (komunikacja wewnątrz procesowa)
// 2. Test Executor przyjmuje assembly studenta i przekazuje je do IUŚ
// 3. Test executro odpytje IUŚ czekając na wynik testu z rozwiązania studenta
// 7. Wyniki zostają zebrane przez Test Executora do plików .json i wykorzystane dalej do wyświetlenia wyniku studentowi 

Z faktu, że system jest w znakomitej większości planowany jako jedna biblioteka, większość komunikacji odbywa się wewnątrzprocesowo, w ramach jednego środowiska uruchomieniowego .NET. Jednak w ramach systemu zdarzają się również inne sposoby komunikacji. Aby przybliżyć ich funkcję i sposób zaprojektowania, prześledźmy uproszczony cykl życia rozwiązania studenta.

Na początek student wypycha zmiany swojego kodu w dedykowanym repozytorium, bądź uruchamia system lokalnie. Kod źródłowy trafia wtedy bezpośrednio do modułu koordynacji testów, czyli Test Executor.

Następnym krokiem jest analiza i kompilacja rozwiązania, a następnie przekazanie skopilowanych bajtów do modułu modyfikacji kodu pośredniego. Zwróćmy uwagę, że odbywa się to wewnątrz tego procesu.

Po zakończeniu pracy przez bibliotekę wstrzykującą kod CIL, koordynator odbiera zaktualizowane bajty programu, a następnie przekazuje je wraz z regułami pomiarowymi do izolowanego środowiska uruchomieniowego. Ta operacja jest zaprojektowana jako osadzenie pliku `.dll` wewnątrz kontenera przez moduł koordynatora. Taki osadzony plik jest przez program uruchamiający widziany jak zwykły plik w systemie plików.

Izolowane środowisko uruchomieniowe uruchamia otrzymane rozwiązanie studenta poprzez odczytanie osadzonego pliku i jego interpretację jako biblioteka. Dzięki temu system macierzysty pozostaje bezpieczny, a zasoby są limitowane. Każdy z testów jest sekwencyjnie wykonywany, a jego wynik zostaje natychmiast zapisywany do pliku w formacie JSON. Z punktu widzenia programu uruchamiającego jest to zapis na dysku, ale _de facto_ jest to zapis na wirtualnym dysku kontenera. Ta metoda komunikacji została wybrana przede wszystkim ze względu na jej prostotę oraz możliwość całkowitego odcięcia kontenera od jego zewnętrza. Sieć ani żaden inny zasób nie jest potrzebny w żadnym momencie wykonywania kodu -- można więc bezpiecznie pozbawić kontener dostępu do nich.

Test Executor, dzięki możliwości odczytywania zawartości kontenera, może sprawdzać obecność plików na jego wirtualnym dysku. Obecność pliku odpowiadającemu danemu testowi jest jednoznaczna z zakończeniem jego wykonywania. Warto tu zauważyć, że może to być zarówno test zakończony powodzeniem, jak i porażką. Sytuację zawieszenia się kontenera bądź przedwczesnego zakończenia jego pracy można wykryć poprzez ustalenie limitu czasu dla całego zestawu testów i jego kontrolę.

Agregacja i zwrot wyników testów planowany jest na dwa sposoby, w zależności od sposobu uruchomienia. W przypadku lokalnego uruchomienia z poziomu IDE, wyniki będą prezentowane jako stan końcowy testu w narzędziu do uruchamiania testów w IDE. Gdy test kończy się powodzeniem, to IDE zaznacza go na zielono, a w przeciwnym przypadku -- na czerwono -- z dodatkowymi informacjami, co poszło nie tak. Natomiast instancje uruchamiane w ramach CI/CD na repozytoriach (np. w serwisie GitHub) będą wyświetlały wyniki jako tekst w logu danej akcji, a ponadto będą wysyłały te wyniki do dedykowanego serwera bądź repozytorium -- katalogu wyników studentów -- w celu dalszego użytkowania tych danych przez prowadzącego i studentów.

== Schematy głównych komponentów  <main-components-description-classes-states-activity-events-diagrams>

Diagram klas widoczny na rysunku @diagram-test-executor ilustruje architekturę modułu TestExecutor oraz jego powiązania z obiektem TestSuite, zawierającym wszystkie obiekty potrzebne do przeprowadzenia testów. Przedstawia kluczowe komponenty modułu, takie jak analizator statyczny, kompilator, narzędzie do wstrzykiwania instrukcji CIL oraz egzekutor, które wspólnie odpowiadają za przetwarzanie rozwiązania studenta i generowanie finalnych wyników.

#figure(
  image("DiagramKlasModuluTestExecutor.png"),
  supplement: "Rysunek",
  caption: [Diagram klas modułu TestExecutor oraz powiązanych komponentów potoku.]
) <diagram-test-executor>

Wykres aktywności widoczny na rysunku @aktywnosc-kompilacja prezentuje współbieżny proces przygotowania logiki testów oraz środowiska uruchomieniowego. Zobrazowany jest tutaj także mechanizm kontroli czasu wykonania. 

#figure(
  image("DiagramAktywnosciProcesuKopilacji.png"),
  supplement: "Rysunek",
  caption: [Diagram aktywności przedstawiający proces kompilacji, wstrzykiwania kodu CIL oraz egzekucji testów.]
) <aktywnosc-kompilacja>

Diagram widoczny na rysunku @stany-executor obrazuje cykl życia głównego obiektu sterującego od jego inicjalizacji do ostatecznego zakończenia sukcesem lub porażką. Szczegółowo rozpisuje na mniejsze podetapy kompilacji i iteracyjnego uruchamiania testów, uwzględniając obsługę wyjątków i przekroczeń limitu czasu.

#figure(
  image("diagram_stanów_testexecutor.drawio.png", height: 85%),
  supplement: "Rysunek",
  caption: [Diagram stanów cyklu życia orkiestracji i egzekucji testów w module TestExecutor.]
) <stany-executor>

Wykres sekwencji widoczny na rysunku @sekwencja modeluje wymianę komunikatów w czasie między interfejsem użytkownika, koordynatorem testów, narzędziami modyfikującymi kod oraz izolowanym środowiskiem uruchomieniowym. Obrazuje sekwencyjne delegowanie zadań kompilacji, wstrzykiwania kodu CIL oraz pętlę odpytywania kontenera o rezultaty wykonania poszczególnych przypadków testowych.

#figure(
  image("DiagraSekwencji.png"),
  supplement: "Rysunek",
  caption: [Diagram sekwencji przedstawiający przepływ sterowania, kompilację, wstrzykiwanie kodu CIL oraz sekwencyjne uruchamianie testów w środowisku izolowanym.]
) <sekwencja>

Diagram aktywności zaprezentowany na rysunku @aktywnosc-kontenera szczegółowo opisuje procesy zachodzące bezpośrednio wewnątrz uruchomionego kontenera testowego. Przedstawia krok po kroku wyszukiwanie klas testowych przez refleksję, izolację wyjścia konsoli oraz bezpieczne wywoływanie funkcji _Arrange_, _Act_ i _Assert_, zakończone serializacją obiektów TestResult do plików JSON.

#figure(
  image("DiagramAktywnosciKontenera.png", height: 90%),
  supplement: "Rysunek",
  caption: [Diagram aktywności dla kontenera testowego.],
) <aktywnosc-kontenera>

//== User interface vision: schemes, plotes, drawings <user-interface-vision-schemes-plotes-drawings>
== Wizja interfejsu użytkownika -- schematy i rysunki <user-interface-vision-schemes-plotes-drawings>

Interfejs platformy został pomyślany tak, by łączyć dwa główne punkty styku z użytkownikiem. Z jednej strony jest to interfejs środowiska programistycznego (IDE), służący do pracy lokalnej, z drugiej zaś interfejs wiersza poleceń (CLI), a także raporty CI/CD, które uruchamiane są w środowisku automatycznej oceny.

// === Interfejs w środowisku IDE (widok studenta i prowadzącego)

W środowisku programistycznym, na przykład w JetBrains Rider czy Visual Studio, testy zdefiniowane w ramach platformy są wykrywane automatycznie. Pojawiają się one w natywnym oknie uruchamiania testów, takim jak Unit Tests Explorer. To możliwe dzięki mechanizmom opartym pod spodem na interfejsie MSTest.

1. Widok eksploratora testów: Każdy przypadek testowy (TestCase) trafia do eksploratora jako osobny test jednostkowy, przypisany do konkretnej kategorii bądź zadania.

2. Prezentacja wyników i asercji: Jeśli test zakończy się niepowodzeniem, na przykład z powodu naruszenia niezmiennika grafu albo przekroczenia dopuszczalnej wartości skompilowanych instrukcji CIL, panel szczegółów pokazuje czytelny komunikat błędu. Opisuje on, czego oczekiwano, a co otrzymano. 

Interfejs użytkownika, jako że platforma jest narzędziem wspierającym proces wytwarzania oprogramowania, nie opiera się na klasycznym interfejsie graficznym (GUI). Punktem styku z użytkownikiem (studentem i prowadzącym) jest środowisko zintegrowane (IDE) za pośrednictwem eksploratora testów, a także konsola (CLI) w zautomatyzowanym środowisku CI/CD.

#figure(
  rect(width: 100%, stroke: 0.5pt + luma(150), inset: 10pt, radius: 4pt)[
    #align(left)[
      #text(weight: "bold", size: 10pt)[ Eksplorator Testów (JetBrains Rider / Visual Studio)]
      #line(length: 100%, stroke: 0.3pt + luma(200))
      #raw(
      "[✓] Lab03Evaluation (ASD2)
 ├── [✓] Case0_Reverse_Gesty (Passed - 12ms)
 ├── [✓] Case1_Reverse_RzadkiDuzy (Passed - 45ms)
 ├── [✗] Case5_Bipartite_Dwudzielny (FAILED - CIL Instruction Limit Exceeded)
 │    └── Message: Expected max 15,000 CIL instructions, but executed 42,108.
 └── [✓] Case10_Kruskal_Maly (Passed - 8ms)",
      lang: "text"
      )
    ]
  ],
  supplement: "Wizualizacja",
  caption: [Szkic wizualizacji wyników testu w oknie eksploratora testów w środowisku IDE],
) <ui-ide-mockup>

W oknie testów IDE prowadzący i student otrzymują natychmiastową informację zwrotną w postaci hierarchicznego drzewa testów. Statusy sukcesu lub błędu są powiązane bezpośrednio z wynikami wyliczonymi przez platformę testową (m.in. weryfikacją liczby wykonanych instrukcji CIL).

#figure(
  rect(width: 100%, stroke: 0.5pt + luma(180), inset: 10pt, radius: 4pt)[
    #align(left)[
      #text(weight: "bold", size: 10pt, font: "DejaVu Sans Mono")[Wyjście CLI / GitHub Actions]
      #line(length: 100%, stroke: 0.3pt + luma(200))
      #raw(
"┌─────────────────────────────┬──────────┬───────────┬──────────────────┐
│ Test Case                   │ Status   │ Time (ms) │ CIL Instructions │
├─────────────────────────────┼──────────┼───────────┼──────────────────┤
│ Case0_Reverse_Gesty         │ PASSED   │ 12        │ 1,240            │
│ Case1_Reverse_RzadkiDuzy    │ PASSED   │ 45        │ 12,890           │
│ Case5_Bipartite_Dwudzielny  │ FAILED   │ 112       │ 42,108 (LIMIT)   │
│ Case10_Kruskal_Maly         │ PASSED   │ 8         │ 850              │
└─────────────────────────────┴──────────┴───────────┴──────────────────┘",
      lang: "text",
      )
    ]
  ],
  supplement: "Wizualizacja",
  caption: [Szkic raportu podsumowującego wyniki i wskaźniki złożoności w konsoli CI/CD],
) <ui-cli-mockup>

W potoku automatyzacji procesów (CI/CD) system wypisuje rezultaty w standardowym wyjściu procesu (stdout). Dane będą mogły być wyświetlone w postaci tabelarycznej, udostępniając agregację pomiarów wydajności i kosztów wyrażonych w instrukcjach języka pośredniego jak pokazuje przykład powyżej. 

= Uzasadnienie wyboru stosu technologicznego <justification-of-technology-stack-choice> 

Poniższa tabelka przedstawia zestawienie kluczowych komponentów architektonicznych oraz narzędzi wybranych oraz ich implementacji. Każda decyzja technologiczna została podjęta na podstawie analizy wymagań stawianych przez klienta, preferencji dotyczących dalszego utrzymania systemu oraz doświadczeń zespołu. Uwzględniono w niej również bilans zysków i strat w stosunku do rozważanych alternatyw.

#figure(
  table(
    columns: (auto, auto, auto, 50%),
    align: horizon,
    stroke: black + 0.5pt,
    
    table.header(
      [*Moduł*],
      [*Wybrana technologia*],
      [*Możliwe \ alternatywy*],
      [*Uzasadnienie*],
    ),
    
    [*Backend \ (moduł \ uruchamiający)*],
    [C\# .NET 10, \ Docker],
    [Rust, C++],
    [Klient, który w przyszłości będzie rozwijać i utrzymywać projekt preferuje język C\#. Członkowie zespołu mają szeroką wiedzę i duże doświadczenie ze środowiskiem .NET. Użycie innego języka niż C\# nie przyniosło by wymiernych korzyści, a podstawowa potrzeba uruchomienia kodu studenta implikuje konieczność użycia środowiska .NET na pewnym etapie działania rozwiązania. Konteneryzacja pozwala w bezpieczny sposób uruchamiać obcy kod, znacząco redukując ryzyko związane z próbami wywołania kodu złośliwego.],
    
    [*Dystrybucja*],
    [Paczka NuGet],
    [Plik biblioteki (`.dll`)],
    [Prosty sposób dystrybucji biblioteki poprzez natywny menedżer pakietów. Pozwala na łatwe aktualizowanie paczki bez konieczności wysyłania żadnych plików studentom. Ponadto integracja poprzez system CI/CD (pkt niżej) całkowicie przejmuje obowiązek wdrożenia nowej wersji od programisty.],

    [*CI/CD*],
    [GitHub \ Actions],
    [Ręczne \ uruchamianie],
    [Automatyczna kompilacja rozwiązania, przeprowadzanie testów poprawności i tworzenie kontenerów Docker, a także wypychanie gotowych artefaktów do repozytoriów DockerHub i NuGet. Brak automatyzacji tego procesu nie przyniósłby żadnych korzyści. Jest to standard branżowy.],

    [*Ocenianie \ i eksport \ wyników*],
    [GitHub \ Actions \ (lub odpowiednik)],
    [Ręczne \ uruchamianie \ przez \ prowadzącego],
    [Automatyczne uruchamianie systemu oceniającego przy każdym wypchnięciu kodu do repozytorium przez studenta. Wysyłanie raportów z wynikami bezpośrednio do dedykowanego repozytorium w celach archiwizacji, wystawiania ocen studentom i prezentacji tych danych na stronie internetowej przedmiotu ASD2. Alternatywą jest ręczne wprowadzanie tych danych do systemu przez prowadzącego, co nie przynosi żadnych korzyści, jest żmudne i może powodować błędy.],
  ),
)

= Wygląd fasady testowej dla prowadzącego <fasada> // TODO: nazwa 
Fasada testowa opera się na wzorcu AAA (_Arrange-Act-Assert_) oraz deklaratywnym podejściu do definiowania zestawów testowych. Prowadzący tworzy zestawy testów  dla konkretnego zadania, pisząc jedną klasę dziedziczącą po klasie zestawu testowego i oznaczając ją odpowiednim atrybutem. Celem fasady jest zminimalizowanie niepotrzebnego, powtarzalnego kodu oraz zapewnienie pełnej automatyzacji weryfikacji rezultatów. Dzięki fasadzie prowadzący nie musi pisać powtarzalnego kodu wielokrotnie, lecz samo meritum testu. 

== Kluczowe komponenty fasady 
1. Prowadzący deklaruje klasę z rozwiązaniami studenta, która będzie tej samej nazwy dostarczona na początku zajęć bez zaimplementowanych funkcji. Dzięki deklaracji Test Executor wie, która klasa jest rozwiązaniem studenckim i gdzie znajdują się zaimplementowane przez studenta funkcje. 

2. Prowadzący posiada możliwość zadeklarowania podstawowej konfiguracji testu. Możliwe będą takie parametry jak możliwość konteneryzacji testu lub maksymalna ilość obliczeń. 

3. Każdy przypadek testowy stanowi osobną, zagnieżdżoną klasę dziedziczącą po klasie przypadku testowego. Wymusza ona podział logiki na trzy czytelne fazy:
  - Przygotowanie (_Arrange_): Przygotowanie testu poprzez stworzenie wymaganych struktur, klas, obiektów potrzebnych do przeprowadzenia testu 
  - Wykonanie akcji (_Act_): Wywołanie właściwej metody z kodu studenta
  - Weryfikacja (_Assert_): Weryfikacja za pomocą dedykowanych metod asercji, sprawdzających nie tylko poprawność wyniku końcowego, ale również efekty uboczne (np. brak spójności grafu itp.). Dzięki modyfikowalnej funkcji Assert() prowadzący nie będzie ograniczony do sprawdzenia jedynie wyniku, lecz także będzie w stanie przetestować implementowalny dowolny warunek. 

== Automatyzacja i generowanie kodu zestawów testowych 
Aby prowadzący nie musiał ręcznie pisać kodu integrującego klasę testową z silnikiem wykonawczym TestExecutor ani tworzyć powtarzalnych metod uruchamiających w środowisku MSTest, platforma wykorzystuje przyrostowy generator kodu kompilatora Roslyn. 
Generator działa całkowicie statycznie w trakcie kompilacji projektu testowego i wykonuje następujące zadania:

=== Filtrowanie i analiza składniowa (Faza Inkrementalna)
W etapie inicjalizacji generator skanuje drzewo składniowe C\# i wyszukuje deklaracje klas (ClassDeclarationSyntax), które spełniają rygorystyczne kryteria architektoniczne:
- Klasa musi być publiczna, nieabstrakcyjna i zadeklarowana na poziomie przestrzeni nazw.
- Klasa musi bezpośrednio lub pośrednio dziedziczyć po typie bazowym TestSuite.

Dla każdej prawidłowej klasy generator odczytuje atrybut [StudentSolution], używając modelu semantycznego Roslyn do pobrania symbolu typu rozwiązania studenta. Następnie wyciąga surowy kod źródłowy drzewa składniowego zadania studenta oraz kod samej klasy testowej. Dodatkowo przeszukuje zagnieżdżone typy w celu zidentyfikowania wszystkich klas przypadków testowych dziedziczących po TestCase.

=== Generowanie kodu wyjściowego
Dla każdego odnalezionego zestawu testowego generator automatycznie tworzy w pamięci kompilatora plik kodowy (`.g.cs`), zawierający dwie kluczowe struktury:

1. Rozszerzenie klasy zestawu testowego, które uzupełnia zadeklarowaną przez prowadzącego klasę o przesłonięte metody dostępowe zwracające surowy kod źródłowy w postaci surowych literałów ciągów znaków 
   #vspace
   ```
   public partial class Lab03Evaluation
   {
       public override string StudentSolutionSourceCode() => """...""";
       public override string TestSuiteSourceCode() => """...""";
   }
   ```
   #vspace
   Pozwala to orkiestratorowi TestExecutor na bezpośredni dostęp do czystych tekstów źródłowych w celu ich późniejszej analizy statycznej i dynamicznej kompilacji Roslyn bez konieczności odczytywania plików z dysku w trakcie wykonywania
#vspace
2. Wydzielona klasa uruchomieniowa MSTest:
  Dla zestawu testowego generowana jest towarzysząca klasa z przyrostkiem \_Tests oznaczona atrybutem [TestClass]. Dla każdego zidentyfikowanego przypadku testowego (TestCase) generator generuje osobną asynchroniczną metodę testową [TestMethod]:
  #vspace

  ```
  [TestClass]
  public partial class Lab03Evaluation_Tests
  {
      private static Lab03Evaluation _suite = new Lab03Evaluation();
      private static TestExecutor _executor = new TestExecutor(_suite);
  
      [TestMethod]
      public async Task Case0_Reverse_Gesty()
      {
          _executor.BeginExecution();
          var result = await _executor.GetResult(0);
          if (result.IsT1)
          {
              var failure = result.AsT1;
              Assert.Fail(failure.Message ?? "Test failed");
          }
      }
      // ... kolejne metody dla każdego przypadku testowego
  }
  ```
  #vspace
  Dzięki tak zaprojektowanemu generatorowi prowadzący pisze wyłącznie zwięzłą klasę deklaratywną AAA, a cały mechanizm orkiestracji, integracji z narzędziami uruchamiania testów w IDE (Visual Studio / JetBrains Rider) oraz asynchronicznego oczekiwania na wyniki z TestExecutor generowany jest automatycznie.

== Przepływ tworzenia zadań przez prowadzącego
Proces weryfikacji i przygotowania zadań przebiega według następujących kroków:
1. Prowadzący definiuje interfejs dla studenta. Student dostaje wyłącznie czyste nagłówki metod i klasę z wymaganą sygnaturą.
2. Prowadzący tworzy zróżnicowane przypadki testowe, które mają na celu weryfikację poprawności kodu studenta. Dla każdego testu definiowana jest klasa przypadku testowego zawierająca dedykowaną logikę sprawdzającą.
3. Studenci implementują rozwiązania zadania. Dzięki testom zdefiniowanym przez prowadzącego mogą w szybki sposób zweryfikować poprawność swojego rozwiązania i testują. 
4. Na koniec studenci dostają raport błędów zwróconych przez sprawdzarkę. W przypadku niepowodzenia fasada zwraca studentowi czytelny komunikat zwrotny o niepowodzeniu kompilacji, przekroczonej ilości operacji lub źle rozwiązanym przypadku. 


