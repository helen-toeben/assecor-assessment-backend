# Overview
This project implements a RESTful Web API for managing person records.  
Data is sourced from a CSV file and exposed as JSON.  
  
The application is structured using a layered architecture with clear separation between Web API, Application logic and Infrastructure.

# Getting started
This project is designed to be built and tested using the .NET CLI without any environment-specific setup beyond a .NET SDK.  
From the repository root the following commands are supported:  
  
Building
```
dotnet build
````
Running the API
```
dotnet run --project WebApi
````
Running tests
```
dotnet test
````

# Accessing the API via Swagger
When running the application in the Development environment an interactive Swagger UI is available at:  
http://localhost:{port}/swagger  
The exact port is displayed in the application startup logs.

# API Endpoints
All endpoints consume and produce application/json.

## Get all persons
```
GET /persons
```
## Get person by id
```
GET /persons/{id}
```
## Get persons by color
```
GET /persons/color/{color}
```
The input for color is case-insensitive and ignores leading or trailing whitespace to ensure predictable behaviour regardless of casing or minor input formatting differences.
## Create a new person
```
POST /persons
```
Creates a new person record and appends it to the CSV data source.  
Request body:
```
{
  "name": "John",
  "lastname": "Doe",
  "zipcode": "12345",
  "city": "Berlin",
  "color": "red"
}
```


# Data handling assumptions
* Incomplete or malformed entries are skipped during parsing.
* Unknown color codes are handled gracefully and are mapped to ```"unbekannt"```

# Architecture
The solution follows a layered design:

## WebApi
* Controllers
* Dependency injections configuration

## Application
* Domain Models
* Repository contracts

## Infrastructure
* CSV-backed repository implementation

# Testing strategy
The project includes
* Unit tests for CSV parsing behaviour. 
* Integration tests using ```WebApplicationFactory``` to verify endpoints end-to-end
Tests override configuration to ensure deterministic execution and avoid reliance on local files.

# Design decisions and trade-offs
* Tolerant parsing:  
Malformed CSV rows are skipped to ensure functionality of the API even when confronted with messy data.
* Load-once:  
The CSV is parsed once and cached in memory for efficiency.
* Repository abstraction:  
Enables replacement of the data source without any changed on the Web API layer.
* Avoiding unnecessary abstractions:  
While implementing the CSV-backed repository I considered introducing dedicated classes for CSV parsing and color mapping.  
Given the limited scope of the challenge I chose to keep this functionality within the repository to avoid premature abstraction. If the parsing logic would grow in complexity or be reused across multiple components, extracting dedicated services would be a natural next step.
* Logging:  
Logging was intentionally not introduced at this stage to keep the implementation focussed on the core requirements.  
In a production system structured logging should typically be added around data loading, parsing failures and unexpected runtime errors.

***
# Assecor Assessment Test (DE)

## Zielsetzung

Das Ziel ist es ein REST – Interface zu implementieren. Bei den möglichen Frameworks stehen .NET(C#), Java oder Go zur Auswahl. Dabei sind die folgenden Anforderungen zu erfüllen:

* Es soll möglich sein, Personen und ihre Lieblingsfarbe über das Interface zu verwalten
* Die Daten sollen aus einer CSV Datei lesbar sein, ohne dass die CSV angepasst werden muss
* Alle Personen mit exakten Lieblingsfarben können über das Interface identifiziert werden

Einige Beispieldatensätze finden sich in `sample-input.csv`. Die Zahlen der ersten Spalte sollen den folgenden Farben entsprechen:

| ID | Farbe |
| --- | --- |
| 1 | blau |
| 2 | grün |
| 3 | violett |
| 4 | rot |
| 5 | gelb |
| 6 | türkis |
| 7 | weiß |

Das Ausgabeformat der Daten ist als `application/json` festgelegt. Die Schnittstelle soll folgende Endpunkte anbieten:

**GET** /persons
```json
[{
"id" : 1,
"name" : "Hans",
"lastname": "Müller",
"zipcode" : "67742",
"city" : "Lauterecken",
"color" : "blau"
},{
"id" : 2,
...
}]
```

**GET** /persons/{id}

*Hinweis*: als **ID** kann hier die Zeilennummer verwendet werden.
```json
{
"id" : 1,
"name" : "Hans",
"lastname": "Müller",
"zipcode" : "67742",
"city" : "Lauterecken",
"color" : "blau"
}
```

**GET** /persons/color/{color}
```json
[{
"id" : 1,
"name" : "Hans",
"lastname": "Müller",
"zipcode" : "67742",
"city" : "Lauterecken",
"color" : "blau"
},{
"id" : 2,
...
}]
```

## Akzeptanzkriterien

1. Die CSV Datei wurde eingelesen, und wird programmintern durch eine dem Schema entsprechende Modellklasse repräsentiert.
2. Der Zugriff auf die Datensätze so abstrahiert, dass eine andere Datenquelle angebunden werden kann, ohne den Aufruf anpassen zu müssen.
3. Die oben beschriebene REST-Schnittstelle wurde implementiert und liefert die korrekten Antworten.
4. Der Zugriff auf die Datensätze, bzw. auf die zugreifende Klasse wird über Dependency Injection gehandhabt.
5.  Die REST-Schnittstelle ist mit Unit-Tests getestet. 
6.  Die `sample-input.csv` wurde nicht verändert 

## Bonuspunkte
* Implementierung als MSBuild Projekt für kontinuierliche Integration auf TFS (C#/.NET) oder als Maven/Gradle Projekt (Java)
* Implementieren Sie eine zusätzliche Methode POST/ Personen, die eine zusätzliche Aufzeichnung zur Datenquelle hinzufügen
* Anbindung einer zweiten Datenquelle (z.B. Datenbank via Entity Framework)

Denk an deine zukünftigen Kollegen, und mach es ihnen nicht zu einfach, indem du deine Lösung öffentlich zur Schau stellst. Danke!

# Assecor Assessment Test (EN)

## goal

You are to implement a RESTful web interface. The choice of framework and stack is yours between .NET (C#), Java or Go. It has to fulfill the following criteria:

* You should be able to manage persons and their favourite colour using the interface
* The application should be able to read the date from the CSV source, without modifying the source file
* You can identify people with a common favourite colour using the interface

A set of sample data is contained within `sample-input.csv`. The number in the first column represents one of the following colours:

| ID | Farbe |
|---|---|
| 1 | blau |
| 2 | grün |
| 3 | violett |
| 4 | rot |
| 5 | gelb |
| 6 | türkis |
| 7 | weiß |

the return content type is `application/json`. The interface should offer the following endpoints:

**GET** /persons
```json
[{
"id" : 1,
"name" : "Hans",
"lastname": "Müller",
"zipcode" : "67742",
"city" : "Lauterecken",
"color" : "blau"
},{
"id" : 2,
...
}]
```

**GET** /persons/{id}

*HINT*: use the csv line number as your **ID**.
```json
{
"id" : 1,
"name" : "Hans",
"lastname": "Müller",
"zipcode" : "67742",
"city" : "Lauterecken",
"color" : "blau"
}
```

**GET** /persons/color/{color}
```json
[{
"id" : 1,
"name" : "Hans",
"lastname": "Müller",
"zipcode" : "67742",
"city" : "Lauterecken",
"color" : "blau"
},{
"id" : 2,
...
}]
```

## acceptance criteria

1. The csv file is read and represented internally by a suitable model class.
2. File access is done with an interface, so the implementation can be easily replaced for other data sources.
3. The REST interface is implemented according to the above specifications.
4. Data access is done using a dependency injection mechanism
5. Unit tests for the REST interface are available.
6. `sample-input.csv` has not been changed.

## bonus points are awarded for the following
* implement the project with MSBuild in mind for CI using TFS/DevOps when using .NET, or as a Maven/Gradle project in Java
* Implement an additional **POST** /persons to add new people to the dataset
* Add a secondary data source (e.g. database via EF or JPA)

Think about your potential future colleagues, and do not make it too easy for them by posting your solution publicly. Thank you!


