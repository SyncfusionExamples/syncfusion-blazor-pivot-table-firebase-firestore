# Blazor Pivot Table - Firebase Firestore Database Connection using ASP.NET Core

## Project Overview

This repository demonstrates how to bind Firebase Firestore data to the Syncfusion Blazor Pivot Table through an ASP.NET Core API and the URL Adaptor. The sample uses the Google.Cloud.Firestore library to load and persist documents from an Orders collection, while the Pivot Table consumes the API through SfDataManager.

The implementation follows the current guide-based pattern where the UI requests data from the server, and the server handles Firestore access, validation, and CRUD operations. This keeps the component focused on aggregation and editing while the API manages persistence.

### Key Features

- Firebase Firestore-backed data source for a lightweight document store sample
- Syncfusion Blazor Pivot Table with URL Adaptor remote binding
- CRUD operations exposed through API endpoints
- Same-origin relative URLs for read and write requests
- Primary key configuration for drill-through editing
- Problem details middleware and exception handling for API failures

## Repository

Sample location: https://github.com/SyncfusionExamples/syncfusion-blazor-pivot-table-firebase-firestore

## Prerequisites

| Component | Version | Purpose |
|-----------|---------|---------|
| .NET SDK | 10.0 | Build and run the sample targeting net10.0 |
| Visual Studio 2026 / VS Code | 18.0+ / latest | Development environment with ASP.NET and web workloads |
| Firebase Project | Active project | Firestore database host with a service account |
| Google.Cloud.Firestore | 4.3.0 | Official Google Cloud Firestore client library for .NET |
| Syncfusion.Blazor.PivotTable | 34.1.33 | Pivot Table UI component |
| Syncfusion.Blazor.Themes | 34.1.33 | Pivot Table styling |
| Newtonsoft.Json | 13.0.4 | JSON serialization for CRUD payloads |

## Firebase Firestore Setup

### 1. Create a Firebase Project

Go to the [Firebase Console](https://console.firebase.google.com/) and create a new project (for example, `pivottablefirestore`).

### 2. Create a Firestore Database

In the Firebase console, navigate to **Firestore Database** and click **Create database**. Choose **Start in production mode** or **test mode** and select a region close to your users.

### 3. Create an Orders collection

Once the database is provisioned, click **Start collection** and name it:

```text
Collection:
Orders
```

### 4. Add sample order documents

Add the following sample documents to the `Orders` collection through the Firebase console **Add document** panel. Each document stores the order fields shown below:

```json
{ "orderId": 1, "customerName": "Toms",   "employeeId": 1, "shipCity": "New York", "freight": 35.30 }
{ "orderId": 2, "customerName": "Ravi",   "employeeId": 2, "shipCity": "London",   "freight": 80.20 }
{ "orderId": 3, "customerName": "Sven",   "employeeId": 1, "shipCity": "Berlin",   "freight": 52.10 }
{ "orderId": 4, "customerName": "Sara",   "employeeId": 3, "shipCity": "Madrid",   "freight": 18.40 }
{ "orderId": 5, "customerName": "Paul",   "employeeId": 2, "shipCity": "Tokyo",    "freight": 64.75 }
```

### 5. Generate a Firebase Service Account key

In the Firebase console, open **Project settings → Service accounts** and click **Generate new private key**. This downloads a JSON credentials file used by the Google.Cloud.Firestore library to authenticate the ASP.NET Core API.

### 6. Place the key file in the project

Rename the downloaded file to `serviceAccountKey.json` and place it in the following folder so the application can locate it at runtime:

```text
Firebase/serviceAccountKey.json
```

### Service Account Key

> **Important:** Do not commit the actual Firebase service account key to source control. Generate your own service account key from the Firebase console and store the file securely.

A dummy example of the credentials file is shown below. Replace the placeholder values with the details from your own Firebase project:

```json
{
  "type": "service_account",
  "project_id": "<YOUR_FIREBASE_PROJECT_ID>",
  "private_key_id": "<PRIVATE_KEY_ID>",
  "private_key": "<PRIVATE_KEY>",
  "client_email": "<CLIENT_EMAIL>",
  "client_id": "<CLIENT_ID>"
}
```

## Project Structure

| File/Folder | Purpose |
|-------------|---------|
| /Controllers/OrderController.cs | API controller exposing read and CRUD endpoints using Google.Cloud.Firestore |
| /Components/Pages/Home.razor | Pivot Table page with URL Adaptor configuration, editing, and drill-through setup |
| /Program.cs | ASP.NET Core and Syncfusion service registration, Firestore credential setup, and middleware |
| /appsettings.json | Firestore ProjectId and collection configuration |
| /Firebase/serviceAccountKey.json | Firebase service account key used for authentication |
| /PivotTableFirestore.csproj | Project file with Syncfusion packages and Google.Cloud.Firestore NuGet references |

## Application Flow

```text
Blazor Pivot Table
        ↓
   SfDataManager
        ↓
    UrlAdaptor
        ↓
  OrderController
        ↓
Google.Cloud.Firestore
        ↓
 Firebase Firestore
        ↓
 Orders Collection
```

1. The Pivot Table sends a request through SfDataManager.
2. The URL Adaptor posts the request to the API endpoint.
3. The controller resolves the request through Google.Cloud.Firestore.
4. The response is returned to the client in the format expected by the Pivot Table.

## NuGet Package

Add the Google.Cloud.Firestore package to the project along with the Syncfusion packages:

```xml
<PackageReference Include="Google.Cloud.Firestore" Version="4.3.0" />
<PackageReference Include="Syncfusion.Blazor.PivotTable" Version="34.1.33" />
<PackageReference Include="Syncfusion.Blazor.Themes" Version="34.1.33" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.4" />
```

## Configuration

### appsettings.json

The application reads the Firestore ProjectId and collection name from appsettings.json:

```json
{
  "FirestoreSettings": {
    "ProjectId": "pivottablefirestore",
    "CollectionName": "Orders"
  }
}
```

### Program.cs

The service account key path is set as the `GOOGLE_APPLICATION_CREDENTIALS` environment variable before the Firestore client is initialized. Only the relevant authentication configuration is shown below:

```csharp
Environment.SetEnvironmentVariable(
    "GOOGLE_APPLICATION_CREDENTIALS",
    Path.Combine(
        builder.Environment.ContentRootPath,
        "Firebase",
        "serviceAccountKey.json"));
```

## Firestore Model

The Order model used by the controller maps .NET properties to Firestore document fields through `[FirestoreProperty]` attributes. The class is decorated with `[FirestoreData]` so the Firestore client can convert documents to and from the type:

```csharp
[FirestoreData]
public class Order
{
    [Key]
    [FirestoreProperty("orderId")]
    public int? OrderID { get; set; }

    [FirestoreProperty("customerName")]
    public string? CustomerName { get; set; }

    [FirestoreProperty("employeeId")]
    public int? EmployeeID { get; set; }

    [FirestoreProperty("freight")]
    public double? Freight { get; set; }

    [FirestoreProperty("shipCity")]
    public string? ShipCity { get; set; }
}
```

## Firestore Document Structure

Each document in the `Orders` collection follows the structure below. The Firestore-generated document ID is used internally by the controller to locate documents for update and delete operations:

```json
{
  "orderId": 1,
  "customerName": "John Smith",
  "employeeId": 101,
  "freight": 32.5,
  "shipCity": "New York"
}
```

## CRUD Operations

The sample supports read, insert, update, and delete operations through the URL Adaptor. The controller uses Google.Cloud.Firestore to query the `Orders` collection and map documents to the Order model.

### Read

The `POST /api/Order` endpoint receives a `DataManagerRequest`, reads all documents from the `Orders` collection using `GetSnapshotAsync()`, converts each document to an Order, and returns the result in the `{ result, count }` format expected by the Pivot Table.

### Insert

The `POST /api/Order/Insert` endpoint receives a `CRUDModel<Order>` containing the new order. The controller calculates the next `orderId` based on the existing documents and adds the order to the collection using `AddAsync`. The created order is returned to the client.

### Update

The `POST /api/Order/Update` endpoint receives a `CRUDModel<Order>` containing the modified order. The controller queries the collection for the document whose `orderId` matches the incoming `OrderID` using `WhereEqualTo`, then overwrites the document with `SetAsync`.

### Delete

The `POST /api/Order/Delete` endpoint receives a `CRUDModel<Order>` whose `key` carries the `orderId` of the document to remove. The controller queries the collection for the matching document and removes it using `DeleteAsync`.

## API Endpoints

| Endpoint | Method | Payload | Description |
|----------|--------|---------|-------------|
| /api/Order | POST | DataManagerRequest | Returns `{ result, count }` for Pivot Table binding |
| /api/Order/Insert | POST | CRUDModel<Order> | Inserts a newly added order and returns the created document |
| /api/Order/Update | POST | CRUDModel<Order> | Updates an existing order document |
| /api/Order/Delete | POST | CRUDModel<Order> | Deletes an order document using the key value |

```text
POST /api/Order
POST /api/Order/Insert
POST /api/Order/Update
POST /api/Order/Delete
```

## Home.razor

The Pivot Table uses same-origin relative URLs and marks OrderID as the primary key for drill-through editing:

```razor
<SfDataManager Url="/api/Order"
               InsertUrl="/api/Order/Insert"
               UpdateUrl="/api/Order/Update"
               RemoveUrl="/api/Order/Delete"
               Adaptor="Adaptors.UrlAdaptor">
</SfDataManager>
```

```csharp
private void BeginDrillThrough(BeginDrillThroughEventArgs args)
{
    for (int i = 0; i < args.GridObj.Columns.Count; i++)
    {
        if (args.GridObj.Columns[i].Field == "OrderID")
        {
            args.GridObj.Columns[i].IsPrimaryKey = true;
        }
        else
        {
            args.GridObj.Columns[i].Visible = true;
        }
    }
}
```

## Important Notes

- The Firebase service account JSON file is required for the application to authenticate with Firestore.
- The JSON file should not be committed to source control. Add it to `.gitignore` and provide it locally.
- Users must generate their own Firebase credentials from the Firebase console.
- The sample uses Firestore collections and documents instead of relational database tables.

## Getting Started

1. Clone or open the repository in Visual Studio or VS Code.
2. Restore packages and build:

   ```bash
   dotnet restore
   dotnet build
   ```

3. Create a Firebase project, provision a Firestore database, and add the `Orders` collection as described above.
4. Generate a service account key and place it at `Firebase/serviceAccountKey.json`.
5. Run the application:

   ```bash
   dotnet run --project PivotTableFirestore
   ```

6. Open the local URL shown in the terminal and verify that the Pivot Table loads and the CRUD endpoints respond correctly.

## Troubleshooting

| Issue | Possible Cause | Resolution |
|-------|----------------|------------|
| Service account authentication issues | `GOOGLE_APPLICATION_CREDENTIALS` points to the wrong path or the key file is invalid | Confirm `Firebase/serviceAccountKey.json` exists in the project root and that the path set in `Program.cs` matches |
| Firestore connection failures | Network restrictions or incorrect ProjectId | Verify the `FirestoreSettings:ProjectId` in `appsettings.json` matches your Firebase project and that outbound HTTPS is allowed |
| Missing Firebase key file | The service account key was not placed in the project | Generate a new key from the Firebase console and copy it to `Firebase/serviceAccountKey.json` |
| Collection not found | The Firestore database or `Orders` collection was not created | Create the `Orders` collection in the Firebase console before running the sample |
| CRUD operation failures | The document does not exist or `orderId` could not be matched | Ensure sample documents are inserted with valid `orderId` values and that the collection name matches `appsettings.json` |
| Firestore permission issues | Security rules block read or write access | Update the Firestore security rules to allow access for the service account, or test in development with open rules |

## Full Documentation

For the complete step-by-step guide, including URL Adaptor configuration, CRUD mappings, and Firebase Firestore connection setup, see the official Syncfusion Blazor Pivot Table documentation.

## Summary

This sample demonstrates how to bind Firebase Firestore data to the Syncfusion Blazor Pivot Table through an ASP.NET Core API and the URL Adaptor. The controller uses Google.Cloud.Firestore to read, insert, update, and delete documents in the `Orders` collection, while the Pivot Table performs aggregation and editing on the client. The same-origin relative URLs and primary key configuration keep the remote binding simple and secure, and the `[FirestoreData]` model attributes map .NET properties to Firestore document fields for clean CRUD handling.
