Ten dokument przybliża proces kompilacji biblioteki standardowej języka C#. Poniższe kroki należy wykonać, aby zaktualizować bazę bibliotek narzędzia, np. w przypadku chęci używania nowszej wersji środowiska .NET. Oryginalnie system sprawdzający został przygotowany dla .NET w wersji 10.

1. Przejdź do repozytorium [dotnet/runtime](https://github.com/dotnet/runtime) i wybierz odpowiednią gałąź `release`. Autorzy niniejszego systemu użyli gałęzi [`release/10.0`](https://github.com/dotnet/runtime/tree/release/10.0), z ostatnim commitem o hashu `425ba4b8e5abe47ff545f7158f95e57e0558d0a5`.

2. Uruchom następującą komendę z katalogu głównego repozytorium. Kompilacja może potrwać kilkanaście minut.
```sh
# Na systemie Windows należy użyć pliku build.cmd
./build.sh -subset clr+libs -c Release /p:EnableCrossgen2=false /p:PublishReadyToRun=false
```
Warto tutaj zauważyć, że domyślnie kompilacja odbywa się dla aktualnie użytkowanej platformy. Więcej informacji na temat dostępnych flag dla tego narzędzia znajduje się [w jego dokumentacji](https://github.com/dotnet/runtime/tree/main/docs/workflow/building).

3. 
