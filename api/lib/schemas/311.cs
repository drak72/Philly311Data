using System.Text.Json;

namespace opendata.lib.schemas {
    /** Type Definitions */
    public class I311Schema
    {
        public static string ToJson() {
            var schema = new Schema
            {
                Columns = [
                    new Column { Name = "Objectid", Type = "INTEGER", Description = "The unique identifier for the record." },
                    new Column { Name = "service_request_id", Type = "INTEGER", Description = "The unique identifier for the service request." },
                    new Column { Name = "subject", Type = "VARCHAR", Description = "The subject of the service request." },
                    new Column { Name = "status", Type = "VARCHAR", Description = "The status of the service request. This can be a value of 'Open' or 'Closed'." },
                    new Column { Name = "status_notes", Type = "VARCHAR", Description = "Additional notes about the status of the service request." },
                    new Column { Name = "service_name", Type = "VARCHAR", Description = "The name of the service request." },
                    new Column { Name = "service_code", Type = "VARCHAR", Description = "Code attached to the service type - format of SR-{Program}{code}" },
                    new Column { Name = "agency_responsible", Type = "VARCHAR", Description = "The agency responsible for the service request." },
                    new Column { Name = "service_notice", Type = "VARCHAR", Description = "Number of business days to respond to the request." },
                    new Column { Name = "requested_datetime", Type = "TIMESTAMP", Description = "The date and time the service request was made." },
                    new Column { Name = "updated_datetime", Type = "TIMESTAMP", Description = "The date and time the service request was last updated." },
                    new Column { Name = "expected_datetime", Type = "TIMESTAMP", Description = "The date and time the service request was expected to be closed." },
                    new Column { Name = "closed_datetime", Type = "TIMESTAMP", Description = "The date and time the service request was closed." },
                    new Column { Name = "address", Type = "VARCHAR", Description = "The address of the service request." },
                    new Column { Name = "zipcode", Type = "INTEGER", Description = "The zip code of the service request." },
                    new Column { Name = "media_url", Type = "VARCHAR", Description = "The media url of images attached to the service request." },
                    new Column { Name = "lat", Type = "DOUBLE", Description = "The latitude of the service request." },
                    new Column { Name = "lon", Type = "DOUBLE", Description = "The longitude of the service request." },
                ]
            };
            
            return JsonSerializer.Serialize(schema);
        }
    }
}