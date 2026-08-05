using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Data;

namespace PivotTableFirestore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly FirestoreDb firestoreDb;
        private readonly string collectionName;

        public OrderController(IConfiguration configuration)
        {
            string projectId =
                configuration["FirestoreSettings:ProjectId"]
                ?? throw new InvalidOperationException(
                    "Firestore ProjectId is not configured.");

            collectionName =
                configuration["FirestoreSettings:CollectionName"]
                ?? "Orders";

            firestoreDb = FirestoreDb.Create(projectId);
        }

        [HttpPost]
        public async Task<object> Post(
            [FromBody] DataManagerRequest request)
        {
            _ = request;

            CollectionReference collection =
                firestoreDb.Collection(collectionName);

            QuerySnapshot snapshot =
                await collection.GetSnapshotAsync();

            List<Order> orders = new();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (!document.Exists)
                {
                    continue;
                }

                Order order = document.ConvertTo<Order>();
                orders.Add(order);
            }

            return new
            {
                result = orders.OrderBy(x => x.OrderID).ToList(),
                count = orders.Count
            };
        }

        [HttpPost("Insert")]
        public async Task<IActionResult> Insert(
            [FromBody] CRUDModel<Order> value)
        {
            if (value.Value is not Order order)
            {
                return BadRequest();
            }

            CollectionReference collection =
                firestoreDb.Collection(collectionName);

            QuerySnapshot snapshot =
                await collection.GetSnapshotAsync();

            int nextOrderId = snapshot.Documents.Count == 0
                ? 1
                : snapshot.Documents
                    .Select(x => x.ConvertTo<Order>())
                    .Max(x => x.OrderID ?? 0) + 1;

            order.OrderID = nextOrderId;

            await collection.AddAsync(order);

            return Ok(order);
        }

        [HttpPost("Update")]
        public async Task<IActionResult> Update(
            [FromBody] CRUDModel<Order> value)
        {
            if (value.Value is not Order order
                || !order.OrderID.HasValue)
            {
                return BadRequest();
            }

            CollectionReference collection =
                firestoreDb.Collection(collectionName);

            QuerySnapshot snapshot =
                await collection
                    .WhereEqualTo("orderId", order.OrderID)
                    .GetSnapshotAsync();

            DocumentSnapshot? document =
                snapshot.Documents.FirstOrDefault();

            if (document is null)
            {
                return NotFound();
            }

            await document.Reference.SetAsync(order);

            return Ok(order);
        }

        [HttpPost("Delete")]
        public async Task<IActionResult> Delete(
            [FromBody] CRUDModel<Order> value)
        {
            if (!int.TryParse(
                value.Key?.ToString(),
                out int orderId))
            {
                return BadRequest();
            }

            CollectionReference collection =
                firestoreDb.Collection(collectionName);

            QuerySnapshot snapshot =
                await collection
                    .WhereEqualTo("orderId", orderId)
                    .GetSnapshotAsync();

            DocumentSnapshot? document =
                snapshot.Documents.FirstOrDefault();

            if (document is null)
            {
                return NotFound();
            }

            await document.Reference.DeleteAsync();

            return NoContent();
        }

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

        public class CRUDModel<T> where T : class
        {
            [JsonPropertyName("action")]
            public string? Action { get; set; }

            [JsonPropertyName("keyColumn")]
            public string? KeyColumn { get; set; }

            [JsonPropertyName("key")]
            public object? Key { get; set; }

            [JsonPropertyName("value")]
            public T? Value { get; set; }

            [JsonPropertyName("added")]
            public List<T>? Added { get; set; }

            [JsonPropertyName("changed")]
            public List<T>? Changed { get; set; }

            [JsonPropertyName("deleted")]
            public List<T>? Deleted { get; set; }

            [JsonPropertyName("params")]
            public IDictionary<string, object>? Params { get; set; }
        }
    }
}