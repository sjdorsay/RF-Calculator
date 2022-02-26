# RF Design Application
A desktop application supporting multiple RF design calculators including trace width/impedance, attenuator resistances, and prototype filter implementations.

This was created using Visual Studio 2019 Version 16.11.2. At present I intend to complete the basic functionality of the filter portion for now it has limited functionality.

The application is split into 3 major sections, and a help guide. The library is intended to deal with storing, and reading data associated to components in the SQL database. The design tool section will contain applets that may be used to solve for required component values. The final section, calculators, is intended for any application used specifically for converting units, or doing basic calculations like IM product.

## Library 
TO DO:
- [x] Implement SQLite adapter for writing/reading to/from the SQLite database
- [ ] Export SQLite database with tool publishing
- [ ] Create S-Parameter viewer

## Design Tools
TO DO:
- [x] Add Transmission Line design tool
- [x] Add Filter design tool
- [x] Add Attenuator design tool
- [ ] Add Power Divider design tool
- [ ] Add Phase Shifter design tool
- [ ] Add coupler design tool

## Calculators
TO DO:
- [ ] Add IP3 / IM Calculator
- [ ] Add Frequency Planning tool
- [ ] Add Link Budget tool


## Help Guide
TO DO:
- [x] Create "help" documentation template
- [ ] Update webpage structure to match application
- [ ] Fill in Library page
- [ ] Fill in Transmission Line page
- Add section for each architecture
- Add applications / notes section
- Add references
- [ ] Fill in Filter page
- Add section for each implementation
- Add applications / notes section
- Add Diplexer / multiplexer section
- Add references
- [ ] Fill in Attenuator page
- Add section for each architecture
- Add applications / notes section
- Add variable attenuator section
- Add Thermal Compensation / Equalizer section
- Add references
- [ ] Fill in Power Divider page
- Add section for each architecture
- Add applications / notes section
- Add references
- [x] Add functionality to help buttons

