# API Overview

This API is built using C# and DuckDB. It handles requests and interacts with a DuckDB database, utilizing AWS services for additional functionality.

## Prerequisites

- .NET SDK
- AWS credentials configured for access to necessary services

## Setup

1. **Clone the repository**:
   ```bash
   git clone <repository-url>
   cd <repository-directory>/api
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Build the project**:
   ```bash
   dotnet build
   ```

4. **Run the API**:
   ```bash
   dotnet run
   ```

## Configuration

- Ensure your AWS credentials are set up correctly. You can configure them using the AWS CLI or by setting environment variables.

## Usage

- The API provides endpoints for interacting with the DuckDB database. Refer to the code for available endpoints and their usage.

## Contributing

Contributions are welcome! Please ensure that your code adheres to the project's coding standards and includes appropriate tests.

## License

This project is licensed under the MIT License. See the LICENSE file for more details. 