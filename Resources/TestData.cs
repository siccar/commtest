using Siccar.Application;
using Siccar.Platform;
using System.Text.Json;
using Action = Siccar.Application.Action;
using System.Text.Json.Nodes;

namespace CommTest.basic
{ 
    public class TestData
    {
        public JsonDocument emptyDoc = JsonDocument.Parse("{}");

        public List<string> testRules = new List<string>();
        public string act2Rule = "2";
        public string trueRule = "{\"!\": [false]}"; // not false
        public string falseRule = "{\"!\": [true]}"; // not true
        public string registerId = "1a3bb4a7e0c34291bbe1343dcf9e2cf2";
        public string blueprintTxId = "942a01515e99e816c193e6d70f4302b904c90e01d5f0ee4f71754c8872848b17";
        public string previousTxId = "942a01515e99e816c193e6d70f4302b904c90e01d5f0ee4f71754c8872848b17";
        public string walletName = "Named Test Wallet";
        public string walletDescription = "Description of the wallets purpose";
        public string walletMnemonic = "swarm ranch mind wash treat latin dash verb elder nice round receive destroy swallow cannon wedding hood old clay bus estate fragile layer skin";
        public string walletAddress = "ws1jzzt5fe9yjx3srx2dx4qvuh4lhn2tadrqhaftpfyvum0ugqxncpcs9pehu8";
        public string walletAddress1 = "ws1jle65qnan70zm4jeuxtvqk9vf3rwaqlzvhmlg5949jmry4g9r376sca3nhv";
        public string walletAddress2 = "ws1jre7tuk2ewjdd4lkpy6ctnj7tmqkdn6dxdxmgcnurcwxnkf6ae8ks97xewg";
        public string walletAddress3 = "ws1jyavckwwkchaj98rvfdh09kmjk6ls6ts529tud58t64yc5mue0ecsjfk7zj";
        public string tenantId = "860739FA-C9B1-4CD2-986B-E5D69EBC6550";
        public string tenantName = "Test Tenant 1";
        public string simpleBlueprintStr()
        {
            return File.ReadAllText("Examples/SimpleBlueprint.json");
        }

        public string simpleWalletStr()
        {
            return File.ReadAllText("Examples/wallet.json");
        }
        public Action action1()
        {
            testRules.Add(trueRule);
            return new Action()
            {
                Id = 1,
                Title = "an action",
                Description = "the act of doing something",
                Blueprint = "blueprintId",
                Disclosures = new List<Disclosure>() { new Disclosure("participant1", new List<string>() { "/item1" }) },
                Condition = act2Rule,
                DataSchemas = new List<JsonDocument>() { emptyDoc },
                Form = new Control(),
                Participants = new List<Condition>() { new Condition("participant1", testRules), new Condition("participant2", testRules) },
                PreviousTxId = blueprintTxId,
                PreviousData = emptyDoc,
                Sender = walletAddress1
            };
        }
        public Action action2()
        {
            testRules.Add(trueRule);
            return new Action()
            {
                Id = 2,
                Title = "a second action",
                Description = "the act of doing the next thing",
                Blueprint = "blueprintId",
                Disclosures = new List<Disclosure>() { new Disclosure("participant3", new List<string>() { "/data2" }) },
                Condition = trueRule,
                DataSchemas = new List<JsonDocument>() { emptyDoc },
                Form = new Control(),
                Participants = new List<Condition>() { new Condition("participant2", testRules), new Condition("participant3", testRules) },
                PreviousTxId = blueprintTxId,
                PreviousData = emptyDoc,
                Sender = walletAddress1
            };
        }
        public Action actionBad()
        {
            return new Action()
            {

                Title = "Its got a title",
                Blueprint = "",
                Condition = falseRule,
                DataSchemas = new List<JsonDocument>() { emptyDoc },
                Description = "",
                Form = null,
                PreviousTxId = "",
                PreviousData = emptyDoc,
                Sender = ""
            };
        }
        public Blueprint blueprint1()
        {
            return new Blueprint()
            {
                Id = "blueprintId",
                Title = "Test Blueprint 1",
                Description = "A test blueprint",
                Version = 1,
                Actions = new List<Action>() { action1() },
                Participants = new List<Participant> { participant1(), participant2() },
                DataSchemas = new List<JsonDocument>() { emptyDoc }
            };
        }
        public string testBlueprintStr = "";
        public Blueprint blueprintBad()
        {
            return new Blueprint()
            {
                Id = "blueprintId",
                Title = "Test Blueprint Bad",
                Description = "A test blueprint with multiple errors",
                Version = 1,
                Actions = new List<Action>() { action1() },
                Participants = new List<Participant> { participant1() },
                DataSchemas = new List<JsonDocument>()
            };
        }
        public Control control1()
        {
            return new Control()
            {
                Title = "A Test Control",
                Scope = "/a/test/path",
                Conditions = new List<JsonNode>() { trueRule },
                ControlType = ControlTypes.Layout,
                Layout = LayoutTypes.VerticalLayout,
                Elements = new List<Control> { },
                Properties = emptyDoc
            };
        }
        public Control controlBad()
        {
            return new Control()
            {
                Title = "A Bad Control",
                Scope = "",
                Conditions = new List<JsonNode>() { falseRule },
                ControlType = ControlTypes.Layout,
                Layout = LayoutTypes.VerticalLayout,
                Elements = new List<Control> { }
            };
        }
        public Condition condition1()
        {
            return new Condition()
            {
                Prinicpal = "dadas",
                Criteria = new List<string>() { trueRule }
            };
        }
        public Condition conditionBad()
        {
            return new Condition()
            {
                Prinicpal = "",
                Criteria = new List<string>()
            };
        }
        public Disclosure disclosure1()
        {
            return new Disclosure()
            {
                ParticipantAddress = walletAddress1,
                DataPointers = new List<string> { "/" }
            };
        }
        public Disclosure disclosureBad()
        {
            return new Disclosure()
            {
                ParticipantAddress = "ws1jre7tuk2ewjdd4lkpy6ctnj7tmqkdn6dxdxmg",
                DataPointers = new List<string> { }
            };
        }
        public Participant participant1()
        {
            return new Participant()
            {
                Id = "mockIdParticipant1",
                Name = "Participant 1",
                Organisation = "Test Org 1",
                didUri = "did://register/participantTxId1",
                WalletAddress = walletAddress1
            };
        }
        public Participant participant2()
        {
            return new Participant()
            {
                Id = "mockIdParticipant2",
                Name = "Participant 2",
                Organisation = "Test Org 2",
                didUri = "did://register/participantTxId2",
                WalletAddress = walletAddress2
            };
        }
        public Participant participant3()
        {
            return new Participant()
            {
                Id = "mockIdParticipant2",
                Name = "Participant 2",
                Organisation = "Test Org 2",
                didUri = "did://register/participantTxId2",
                useStealthAddress = true,
                WalletAddress = walletAddress3
            };
        }
        public Participant participantBad()
        {
            return new Participant()
            {
                Id = "",
                Name = "",
                Organisation = "too large and orgname ....",
                didUri = "bad-diduri",
                useStealthAddress = true,
                WalletAddress = "badAddress"
            };
        }

        public Tenant tenant1()
        {
            return new Tenant()
            {
                Id = tenantId,
                Name = tenantName,
                AdminEmail = "test@test.com",
                Registers = new List<string> { registerId }
            };
        }

        
    }
}
