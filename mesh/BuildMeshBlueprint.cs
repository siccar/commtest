using Siccar.Application;
using Siccar.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Action = Siccar.Application.Action;

namespace CommTest.mesh
{
    public class BuildMeshBlueprint
    {
        JsonSerializerOptions jopts = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        /// <summary>
        /// Build Mesh Blueprint
        /// 
        /// This one will auto build a template
        /// </summary>
        /// <param name="template"></param>
        /// <param name="testParticipants"></param>
        /// <returns></returns>
        public Blueprint Build(List<Participant> testParticipants)
        {
            Blueprint blueprint = new Blueprint()
            {
                Title = "Mesh Blueprint - " + DateTime.Now.ToString(),
                Description = "A fully meshed transaction load generator",
                Version = 1
            };

            // read the data schemas from file
            try
            {

                var data_schemas = JsonSerializer.Deserialize<JsonDocument>(File.ReadAllText("mesh/array_data_schemas.json"),
                    new JsonSerializerOptions(JsonSerializerDefaults.Web));

                var ds = data_schemas.RootElement.GetProperty("dataSchemas");
                blueprint.DataSchemas = JsonSerializer.Deserialize<List<JsonDocument>>(ds);
            }
            catch (Exception er)
            {
                Console.WriteLine(er.Message);

            }


            // Setup the Participants
            var disclosures = new List<Disclosure>();
            var allParticipants = new List<string>();
            disclosures.Add(JsonSerializer.Deserialize<Disclosure>("{\"participantAddress\": \"TrackingData\",\"dataPointers\": [\"endorse\", \"ballast\"]}", jopts));


            foreach (var p in testParticipants)
            {
                blueprint.Participants.Add(p);
                disclosures.Add(JsonSerializer.Deserialize<Disclosure>("{\"participantAddress\": \"" + p.Id + "\",\"dataPointers\": [\"endorse\", \"ballast\"]}", jopts));
                allParticipants.Add(p.Id);
            }

            // setup the action participants 
            List<Condition> actionParticipant = new List<Condition>();
            foreach (var p in testParticipants)
            {
                actionParticipant.Add(new Condition(p.Id, true));
            }

            // Arrange the actions we are going to use 2
            // A1 triggers A2 -> A2

            Action triggerAction = new Action()
            {
                Id = 1,
                Blueprint = blueprint.Id,
                PreviousTxId = "000000000000000000000000000000000",
                DataSchemas = blueprint.DataSchemas,
                Disclosures = disclosures,
                Title = "Mesh Trigger Transaction",
                Description = "Mesh Trigger Transaction",
                Sender = testParticipants.First().Id,
                AdditionalRecipients = allParticipants,
                Condition = JsonNode.Parse("{ \"or\": [ false, 1 ] }")
            };

            Action repeatingAction = new Action()
            {
                Id = 2,
                Blueprint = blueprint.Id,
                PreviousTxId = "000000000000000000000000000000000",
                DataSchemas = blueprint.DataSchemas,
                Disclosures = disclosures,
                Title = "Mesh TX (i) to (!i)",
                Description = "Mesh Repeating Transaction",
                AdditionalRecipients = allParticipants,
                Condition = JsonNode.Parse("{ \"or\": [ false, 1 ] }")
            };

            blueprint.Actions.Add(triggerAction);
            blueprint.Actions.Add(repeatingAction);


            return blueprint;
        }
    }
}