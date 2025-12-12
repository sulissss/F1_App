# Database Structure Documentation

This document outlines the database schema for the F1 Application, optimized for SQL Server and designed with **3rd Normal Form (3NF)** principles.

## Core Tables

### 1. `Season`
Stores information about each F1 season.
- **PK**: `season_id`
- `year` (INT, Unique): The year of the season (e.g., 2024).
- `status`: 'active', 'planned', or 'completed'.

### 2. `Country` (New for 3NF)
Centralizes country data to remove transitive dependencies from `Circuit`, `Event`, and `Driver`.
- **PK**: `country_code` (VARCHAR(3), e.g., 'GBR', 'USA').
- `name`: Full country name.
- `flag_image` (VARBINARY(MAX)): Stores the country's flag image.

### 3. `Circuit`
Details about the race tracks.
- **PK**: `circuit_id`
- `circuit_key`: OpenF1 external key.
- `country_code` (FK -> Country): Reference to the country.
- `name`, `city`, `location`, `length_km`: Circuit details.

### 4. `Event`
Represents a Grand Prix weekend.
- **PK**: `event_id`
- `season_id` (FK -> Season): links to the season.
- `meeting_key`: OpenF1 external key.
- `country_code` (FK -> Country): Reference to country (was separate string before 3NF).
- `gp_name`: e.g., "Bahrain Grand Prix".

### 5. `Session`
Specific sessions within an event (FP1, Quali, Race).
- **PK**: `session_id`
- `event_id` (FK -> Event).
- `session_type`: 'Race', 'Qualifying', etc.

### 6. `Team`
F1 Constructors.
- **PK**: `team_id`
- `name`: Full team name (e.g., "Red Bull Racing").
- `country_code` (FK -> Country): Reference to origin country.
- `colour_hex`: Official team color (e.g., #3671C6).
- `logo_image` (VARBINARY(MAX)): Stores the team logo image.

### 7. `Driver`
F1 Drivers.
- **PK**: `driver_id`
- `driver_number`: Permanent car number.
- `code`: Valid 3-letter abbreviation (e.g., VER).
- `country_code` (FK -> Country): Reference to nationality.
- `headshot_image` (VARBINARY(MAX)): Stores the driver's headshot.
- `full_name`: Concatenated name.

---

## Relationships & 3NF Compliance

- **Country Table**: Introduced to normalize country data. `Circuit`, `Event`, and `Driver` now reference `Country` via `country_code` instead of storing the string `country_name` individually, preventing update anomalies.
- **Foreign Keys**: All relationships are enforced with FK constraints (e.g., `Session -> Event -> Season`).
- **Image Storage**: Images are stored directly in the database as BLOBs (`VARBINARY(MAX)`), ensuring data portability and consistency with the entity record.

---

## Views

- `vw_DriverStandings`: Calculates current championship standings based on `SessionResult`.
- `vw_TeamStandings`: Calculates constructor standings.
- `vw_RaceResults`: Simplified view for race result display.
- `vw_QualifyingResults`: Simplified view for qualifying.
- `vw_Last5Races`: Performance trend analysis.

## Stored Procedures

- `sp_GetDriverPerformance`: Aggregates stats for a specific driver.
- `sp_GetHeadToHead`: Compares two drivers' race results.
- `sp_GetCircuitStats`: Historical stats for a specific track.
