using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Qubisoft.Vision6.Models;

namespace Qubisoft.Vision6.Test
{

    [TestClass]
    public class Vision6UnitTests
    {

        private Vision6Client? v6client;
        private string? apiKey;
        // the list we are using for tests
        private int listId;


        // initialise the client so that we don't have to in below unit tests except for the authentication tests
        [TestInitialize]
        public async Task Initialize()
        {
            v6client = new Vision6Client();
            // ensure appsettings.test.json is in Vision6Testing project root
            IConfigurationRoot config = new ConfigurationBuilder().AddJsonFile("appsettings.test.json").Build();
            try {
                apiKey = config["api:key"];
                // id of a list that is used in a lot of the test cases.
                // The list should be created with values as seen in the test cases for the tests to pass
                listId = int.Parse(config["list:mainTestListId"]);
            } catch  {
                throw new Exception("Please provide correct environment variables for api key and mainTestListId. Also ensure appsettings.test.json is in Vision6Testing project root.");
            }

            await v6client.Authenticate(apiKey);

        }

        [TestMethod]
        public async Task AuthenticateTestValidKeyAsync()
        {
            Vision6Client v6clientAutenticate = new Vision6Client();
            bool result = await v6clientAutenticate.Authenticate(apiKey);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task AuthenticateTestInvalidKeyAsync()
        {
            Vision6Client v6clientAutenticate = new Vision6Client();
            bool result = true;
            try {
                await v6clientAutenticate.Authenticate("dsafdsfdsfds");
            } catch {
                result = false;
            }
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task GetContacts()
        {
            Contact contact1 = new Contact
            {
                email = "freddy@test.com",
                mobile = "61432467892",
                first_name = "Fred",
                last_name = "Top",
                password = "password"

            };
            Contact contact2 = new Contact
            {
                email = "timmy@gmail.com",
                mobile = "61425643789",
                first_name = "Tim",
                last_name = "Bobby",
                password = "password"
            };
            var response = await v6client.GetContacts(listId, null, new List<string> { "mobile", "email", "first_name", "last_name", "password" });
            bool result = true;

            int count = 0;
            // go through first two results which should equal contact1 and contact2
            foreach (var contactJObject in response) {
                if (count == 0) {
                    foreach (JProperty property in contactJObject) {
                        string key = property.Name;
                        JToken value = property.Value;
                        if (key == "email")
                        {
                            result = value.ToString() == contact1.email;
                        }
                        else if (key == "mobile")
                        {
                            result = value.ToString() == contact1.mobile;
                        }
                        else if (key == "first_name")
                        {
                            result = value.ToString() == contact1.first_name;
                        }
                        else if (key == "last_name")
                        {
                            result = value.ToString() == contact1.last_name;
                        }
                        else if (key == "password")
                        {
                            result = value.ToString() == contact1.password;
                        }
                    }
                }
                else if (count == 1) {
                    foreach (JProperty property in contactJObject)
                    {
                        string key = property.Name;
                        JToken value = property.Value;
                        if (key == "email")
                        {
                            result = value.ToString() == contact2.email;
                        }
                        else if (key == "mobile")
                        {
                            result = value.ToString() == contact2.mobile;
                        }
                        else if (key == "first_name")
                        {
                            result = value.ToString() == contact2.first_name;
                        }
                        else if (key == "last_name")
                        {
                            result = value.ToString() == contact2.last_name;
                        }
                        else if (key == "password")
                        {
                            result = value.ToString() == contact2.password;
                        }
                    }
                }
                else break;
                count++;
            }
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task GetContact()
        {
            Contact contact = new Contact
            {
                email = "freddy@test.com",
                mobile = "61432467892",
                first_name = "Fred",
                last_name = "Top",
                password = "password"
            };
            var response = await v6client.GetContact(listId, 1, new List<string> { "mobile", "email" });
            bool result = true;

            int count = 0;
            // go through first two results which should equal contact1 and contact2
            foreach (JProperty property in response)
            {
                string key = property.Name;
                JToken value = property.Value;
                if (key == "email")
                {
                    result = value.ToString() == contact.email;
                }
                else if (key == "mobile")
                {
                    result = value.ToString() == contact.mobile;
                }
                else if (key == "first_name")
                {
                    result = value.ToString() == contact.first_name;
                }
                else if (key == "last_name")
                {
                    result = value.ToString() == contact.last_name;
                }
                else if (key == "password")
                {
                    result = value.ToString() == contact.password;
                }
            }
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task DeleteContact()
        {
            Contact contactToDelete = new Contact
            {
                email = "anewcontacttodelete@test.com",
                mobile = "61465468977",
            };
            await v6client.CreateContact(listId, contactToDelete);
            var responseGetContacts = await v6client.GetContacts(listId, null, new List<string> { "id", "mobile", "email" });
            var newlyAddedContact = responseGetContacts.Last;

            string idToDelete = "0";
            bool result = true;
            foreach (JProperty property in newlyAddedContact)
            {
                string key = property.Name;
                JToken value = property.Value;

                if (key == "id") {
                    idToDelete = value.ToString();
                }
                if (key == "email")
                {
                    if (value.ToString() != contactToDelete.email)
                        result = false;
                }
                else if (key == "mobile")
                {
                    if (value.ToString() != contactToDelete.mobile)
                        result = false;
                }
            }
            // if result is not true here then the test fails because the details above should match the contact to delete
            if (result) {
                await v6client.DeleteContact(listId, int.Parse(idToDelete));
                var responseGetContactsAfterDeletion = await v6client.GetContacts(listId, null, new List<string> { "mobile", "email" });
                foreach (JProperty property in responseGetContactsAfterDeletion.Last)
                {
                    string key = property.Name;
                    JToken value = property.Value;
                    if (key == "email")
                    {
                        result = value.ToString() != contactToDelete.email;
                    }
                    else if (key == "mobile")
                    {
                        result = value.ToString() != contactToDelete.mobile;
                    }
                }
            }
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task DeleteContacts()
        {
            Contact contactToDelete1 = new Contact
            {
                email = "anewcontacttodelete1@test.com",
                mobile = "61465468999",
                first_name = "delete1",
                last_name = "deletey1",
                password = "password1",
            };
            Contact contactToDelete2 = new Contact
            {
                email = "anewcontacttodelete2@test.com",
                mobile = "61465468977",
                first_name = "delete2",
                last_name = "deletey2",
                password = "password2",
            };
            await v6client.CreateContact(listId, contactToDelete1);
            var responseGetContacts1 = await v6client.GetContacts(listId, null, new List<string> { "id", "mobile", "email" });
            await v6client.CreateContact(listId, contactToDelete2);
            var responseGetContacts2 = await v6client.GetContacts(listId, null, new List<string> { "id", "mobile", "email" });
            var newlyAddedContact1 = responseGetContacts1.Last;
            var newlyAddedContact2 = responseGetContacts2.Last;

            string idToDelete1 = "0";
            string idToDelete2 = "0";
            bool result = true;
            foreach (JProperty property in newlyAddedContact1)
            {
                string key = property.Name;
                JToken value = property.Value;

                if (key == "id")
                {
                    idToDelete1 = value.ToString();
                }
                if (key == "email")
                {
                    if (value.ToString() != contactToDelete1.email)
                        result = false;
                }
                else if (key == "mobile")
                {
                    if (value.ToString() != contactToDelete1.mobile)
                        result = false;
                }
                else if (key == "first_name")
                {
                    if (value.ToString() != contactToDelete1.first_name)
                        result = false;
                }
                else if (key == "last_name")
                {
                    if (value.ToString() != contactToDelete1.last_name)
                        result = false;
                }
                else if (key == "password")
                {
                    if (value.ToString() != contactToDelete1.password)
                        result = false;
                }
            }

            foreach (JProperty property in newlyAddedContact2)
            {
                string key = property.Name;
                JToken value = property.Value;

                if (key == "id")
                {
                    idToDelete2 = value.ToString();
                }
                if (key == "email")
                {
                    if (value.ToString() != contactToDelete2.email)
                        result = false;
                }
                else if (key == "mobile")
                {
                    if (value.ToString() != contactToDelete2.mobile)
                        result = false;
                }
                else if (key == "first_name")
                {
                    if (value.ToString() != contactToDelete2.first_name)
                        result = false;
                }
                else if (key == "last_name")
                {
                    if (value.ToString() != contactToDelete2.last_name)
                        result = false;
                }
                else if (key == "password")
                {
                    if (value.ToString() != contactToDelete2.password)
                        result = false;
                }
            }
            // if result is not true here then the test fails because the details above should match the contact to delete
            if (result)
            {
                List<int> idsToDeleteList = new List<int>() {
                    int.Parse(idToDelete1), 
                    int.Parse(idToDelete2),
                };
                await v6client.DeleteContacts(listId, idsToDeleteList);
                var responseGetContactsAfterDeletion = await v6client.GetContacts(listId, null, new List<string> { "mobile", "email" });
                foreach (JProperty property in responseGetContactsAfterDeletion.Last)
                {
                    string key = property.Name;
                    JToken value = property.Value;
                    if (key == "email")
                    {
                        result = value.ToString() != contactToDelete1.email && value.ToString() != contactToDelete2.email;
                    }
                    else if (key == "mobile")
                    {
                        result = value.ToString() != contactToDelete1.mobile && value.ToString() != contactToDelete2.mobile;
                    }
                    else if (key == "first_name")
                    {
                        result = value.ToString() != contactToDelete1.first_name && value.ToString() != contactToDelete2.first_name;
                    }
                    else if (key == "last_name")
                    {
                        result = value.ToString() != contactToDelete1.last_name && value.ToString() != contactToDelete2.last_name;
                    }
                    else if (key == "password")
                    {
                        result = value.ToString() != contactToDelete1.password && value.ToString() != contactToDelete2.password;
                    }
                }
            }
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task CreateContact()
        {
            Contact newContact = new Contact
            {
                email = "anewcontact@test.com",
                mobile = "61432468977",
                first_name = "Test",
                last_name = "Test",
                password = "password",
            };
            await v6client.CreateContact(listId, newContact);
            bool result = false;
            string newlyCreatedContactId = "0";
            var responseGetContactsAfterCreation = await v6client.GetContacts(listId, null, new List<string> { "id", "mobile", "email", "first_name", "last_name", "password" });
            foreach (JProperty property in responseGetContactsAfterCreation.Last)
            {
                string key = property.Name;
                JToken value = property.Value;
                if (key == "id") { 
                    newlyCreatedContactId = value.ToString();
                }
                else if (key == "email")
                {
                    result = value.ToString() == newContact.email;
                }
                else if (key == "mobile")
                {
                    result = value.ToString() == newContact.mobile;
                }
                else if (key == "first_name")
                {
                    result = value.ToString() == newContact.first_name;
                }
                else if (key == "last_name")
                {
                    result = value.ToString() == newContact.last_name;
                }
                else if (key == "password")
                {
                    result = value.ToString() == newContact.password;
                }
            }
            if (result)
                await v6client.DeleteContact(listId, int.Parse(newlyCreatedContactId));
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task UpdateContact()
        {
            Contact newContactToUpdate = new Contact
            {
                email = "anewcontacttoupdate@test.com",
                mobile = "61432468977",
                first_name = "TestUpdate",
                last_name = "TestUpdate",
                password = "password",
            };
            await v6client.CreateContact(listId, newContactToUpdate);

            bool result = false;
            string newlyCreatedContacttoUpdateId = "0";
            var response = await v6client.GetContacts(listId, null, new List<string> { "id", "mobile", "email", "first_name", "last_name", "password" });
            foreach (JProperty property in response.Last)
            {
                string key = property.Name;
                JToken value = property.Value;
                if (key == "id")
                {
                    newlyCreatedContacttoUpdateId = value.ToString();
                }
                else if (key == "email")
                {
                    result = value.ToString() == newContactToUpdate.email;
                }
                else if (key == "mobile")
                {
                    result = value.ToString() == newContactToUpdate.mobile;
                }
            }

            Contact contactUpdateData = new Contact
            {
                email = "anewcontacttoupdatedifferent@test.com",
                mobile = "61432489075",
                first_name = "diffName",
                last_name = "diffLastName",
                password = "diffPassword",
            };

            await v6client.UpdateContact(listId, int.Parse(newlyCreatedContacttoUpdateId), contactUpdateData);
            var responseAfterUdate = await v6client.GetContacts(listId, null, new List<string> { "id", "mobile", "email", "first_name", "last_name", "password" });

            foreach (JProperty property in responseAfterUdate.Last)
            {
                string key = property.Name;
                JToken value = property.Value;
                if (key == "email")
                {
                    result = value.ToString() == contactUpdateData.email;
                }
                else if (key == "mobile")
                {
                    result = value.ToString() == contactUpdateData.mobile;
                }
                else if (key == "first_name")
                {
                    result = value.ToString() == contactUpdateData.first_name;
                }
                else if (key == "last_name")
                {
                    result = value.ToString() == contactUpdateData.last_name;
                }
                else if (key == "password")
                {
                    result = value.ToString() == contactUpdateData.password;
                }

            }

          if (result)
                await v6client.DeleteContact(listId, int.Parse(newlyCreatedContacttoUpdateId));
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task UpdateContacts()
        {
            Contact newContactToUpdate1 = new Contact
            {
                email = "anewcontacttoupdate1@test.com",
                mobile = "61432468977",
                first_name = "TestUpdate1",
                last_name = "TestUpdate1",
                password = "password1",
            };
            Contact newContactToUpdate2 = new Contact
            {
                email = "anewcontacttoupdate2@test.com",
                mobile = "61432468966",
                first_name = "TestUpdate2",
                last_name = "TestUpdate2",
                password = "password2",
            };
            await v6client.CreateContact(listId, newContactToUpdate1);
            var responseGetContacts1 = await v6client.GetContacts(listId, null, new List<string> { "id", "mobile", "email", "first_name", "last_name", "password" });
            await v6client.CreateContact(listId, newContactToUpdate2);
            var responseGetContacts2 = await v6client.GetContacts(listId, null, new List<string> { "id", "mobile", "email", "first_name", "last_name", "password" });



            bool result = false;
            string newlyCreatedContacttoUpdateId1 = "0";
            string newlyCreatedContacttoUpdateId2 = "0";
            foreach (JProperty property in responseGetContacts1.Last)
            {
                string key = property.Name;
                JToken value = property.Value;
                if (key == "id")
                {
                    newlyCreatedContacttoUpdateId1 = value.ToString();
                }
                else if (key == "email")
                {
                    result = value.ToString() == newContactToUpdate1.email;
                }
                else if (key == "mobile")
                {
                    result = value.ToString() == newContactToUpdate1.mobile;
                }
            }

            foreach (JProperty property in responseGetContacts2.Last)
            {
                string key = property.Name;
                JToken value = property.Value;
                if (key == "id")
                {
                    newlyCreatedContacttoUpdateId2 = value.ToString();
                }
                else if (key == "email")
                {
                    result = value.ToString() == newContactToUpdate2.email;
                }
                else if (key == "mobile")
                {
                    result = value.ToString() == newContactToUpdate2.mobile;
                }
            }
            if (result) {
                List<Contact> contactList = new List<Contact>
                {
                    new Contact
                    {
                        id = int.Parse(newlyCreatedContacttoUpdateId1),
                        email = "anewcontacttoupdatedifferent1@test.com",
                        mobile = "61432489099",
                        first_name = "diffName1",
                        last_name = "diffLastName1",
                        password = "diffPassword1",
                    },
                    new Contact
                    {
                        id = int.Parse(newlyCreatedContacttoUpdateId2),
                        email = "anewcontacttoupdatedifferent2@test.com",
                        mobile = "61432489088",
                        first_name = "diffName2",
                        last_name = "diffLastName2",
                        password = "diffPassword2",
                    }
                };

                await v6client.UpdateContacts(listId, contactList);
                var responseAfterUpdate1 = await v6client.GetContacts(listId, null, new List<string> { "id", "mobile", "email", "first_name", "last_name", "password" });
                var updatedContact2 = responseAfterUpdate1.Last;
                await v6client.DeleteContact(listId, int.Parse(newlyCreatedContacttoUpdateId2));

                var responseAfterUpdate2 = await v6client.GetContacts(listId, null, new List<string> { "id", "mobile", "email", "first_name", "last_name", "password" });
                var updatedContact1 = responseAfterUpdate2.Last;
                await v6client.DeleteContact(listId, int.Parse(newlyCreatedContacttoUpdateId1));

                foreach (JProperty property in updatedContact1)
                {
                    string key = property.Name;
                    JToken value = property.Value;
                    if (key == "email")
                    {
                        result = value.ToString() == contactList[0].email;
                    }
                    else if (key == "mobile")
                    {
                        result = value.ToString() == contactList[0].mobile;
                    }
                    else if (key == "first_name")
                    {
                        result = value.ToString() == contactList[0].first_name;
                    }
                    else if (key == "last_name")
                    {
                        result = value.ToString() == contactList[0].last_name;
                    }
                    else if (key == "password")
                    {
                        result = value.ToString() == contactList[0].password;
                    }

                }

                foreach (JProperty property in updatedContact2)
                {
                    string key = property.Name;
                    JToken value = property.Value;
                    if (key == "email")
                    {
                        result = value.ToString() == contactList[1].email;
                    }
                    else if (key == "mobile")
                    {
                        result = value.ToString() == contactList[1].mobile;
                    }
                    else if (key == "first_name")
                    {
                        result = value.ToString() == contactList[1].first_name;
                    }
                    else if (key == "last_name")
                    {
                        result = value.ToString() == contactList[1].last_name;
                    }
                    else if (key == "password")
                    {
                        result = value.ToString() == contactList[1].password;
                    }

                }
            }
            else {
                await v6client.DeleteContact(listId, int.Parse(newlyCreatedContacttoUpdateId1));
                await v6client.DeleteContact(listId, int.Parse(newlyCreatedContacttoUpdateId2));
            }

            Assert.IsTrue(result);
        }


        [TestMethod]
        public async Task GetLists()
        {
            string EXPECTED_LIST_NAME_1 = "Blank List";
            string EXPECTED_LIST_NAME_2 = "Quick Send";
            string EXPECTED_LIST_NAME_3 = "General Enquiries";
            bool result = true;
            var listResponse = await v6client.GetLists();

            foreach (var contactJObject in listResponse)
            {
                foreach (JProperty property in contactJObject)
                {
                    string key = property.Name;
                    JToken value = property.Value;
                    if (key == "name")
                    {
                        result = value.ToString() == EXPECTED_LIST_NAME_1 || value.ToString() == EXPECTED_LIST_NAME_2 || value.ToString() == EXPECTED_LIST_NAME_3;
                    }
                }
            }

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task GetListsWithFilter()
        {
            string EXPECTED_LIST_NAME = "Blank List";
            bool result = true;
            Dictionary<string, string> listFilters = new Dictionary<string, string>()
            {
                { "name", EXPECTED_LIST_NAME}
            };

            var listResponse = await v6client.GetLists(listFilters);

            foreach (var contactJObject in listResponse)
            {
                foreach (JProperty property in contactJObject)
                {
                    string key = property.Name;
                    JToken value = property.Value;
                    if (key == "name")
                    {
                        result = value.ToString() == EXPECTED_LIST_NAME;
                    }
                }
            }

            // should only be one list called Blank List
            if (listResponse.Count != 1)
                result = false;

            Assert.IsTrue(result);
        }


        [TestMethod]
        public async Task GetList()
        {
            string EXPECTED_LIST_NAME = "Blank List";
            bool result = false;
            Dictionary<string, string> listFilters = new Dictionary<string, string>()
            {
                { "name", EXPECTED_LIST_NAME}
            };

            var listResponse = await v6client.GetLists(listFilters);
            int listId = 0;

            foreach (var contactJObject in listResponse)
            {
                foreach (JProperty property in contactJObject)
                {
                    string key = property.Name;
                    JToken value = property.Value;

                    if (key == "id")
                    {
                        listId = int.Parse(value.ToString());
                    }
                }
            }

            if (listId != 0) {
                var getListResponse = await v6client.GetList(listId);

                foreach (JProperty property in getListResponse)
                {
                    string key = property.Name;
                    JToken value = property.Value;

                    if (key == "name")
                    {
                        result = value.ToString() == EXPECTED_LIST_NAME;
                    }
                }

            }

            Assert.IsTrue(result);
        }

        // for this test to work should only have two messages in the VISION6 system which have details as defined in the constants
        [TestMethod]
        public async Task GetMessages()
        {
            bool result = true;
            var EXPECTED_MESSAGE_NAME_1 = "A test Plain Message";
            var EXPECTED_MESSAGE_NAME_2 = "A Test Ecommerce Message";
            var EXPECTED_SUBJECT_NAME_1 = "Test Plain Message";
            var EXPECTED_SUBJECT_NAME_2 = "Test Ecommerce Message";
            var EXPECTED_TYPE = "email";

            var response = await v6client.GetMessages();

            foreach (var messageJObject in response)
            {
                foreach (JProperty property in messageJObject)
                {
                    string key = property.Name;
                    JToken value = property.Value;

                    if (key == "name")
                    {
                        result = value.ToString() == EXPECTED_MESSAGE_NAME_1 || value.ToString() == EXPECTED_MESSAGE_NAME_2;
                    }
                    else if (key == "subject")
                    {
                        result = value.ToString() == EXPECTED_SUBJECT_NAME_1 || value.ToString() == EXPECTED_SUBJECT_NAME_2;
                    }
                    else if (key == "type")
                    {
                        result = value.ToString() == EXPECTED_TYPE;
                    }

                    if (!result)
                        break;
                }
            }


            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task GetMessagesWithFilter()
        {
            bool result = true;
            var EXPECTED_MESSAGE_NAME = "A test Plain Message";
            var EXPECTED_SUBJECT_NAME = "Test Plain Message";
            var EXPECTED_TYPE = "email";

            Dictionary<string, string> listFilters = new Dictionary<string, string>()
            {
                { "name", EXPECTED_MESSAGE_NAME}
            };

            var response = await v6client.GetMessages(listFilters);

            foreach (var messageJObject in response)
            {
                foreach (JProperty property in messageJObject)
                {
                    string key = property.Name;
                    JToken value = property.Value;

                    if (key == "name")
                    {
                        result = value.ToString() == EXPECTED_MESSAGE_NAME;
                    }
                    else if (key == "subject")
                    {
                        result = value.ToString() == EXPECTED_SUBJECT_NAME;
                    }
                    else if (key == "type")
                    {
                        result = value.ToString() == EXPECTED_TYPE;
                    }

                    if (!result)
                        break;
                }
            }

            if (response.Count != 1)
                result = false;


            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task GetMessage()
        {
            bool result = true;
            var EXPECTED_MESSAGE_NAME = "A test Plain Message";
            var EXPECTED_SUBJECT_NAME = "Test Plain Message";
            var EXPECTED_TYPE = "email";

            Dictionary<string, string> listFilters = new Dictionary<string, string>()
            {
                { "name", EXPECTED_MESSAGE_NAME}
            };

            var response = await v6client.GetMessages(listFilters);
            int messageId = 0;

            foreach (var messageJObject in response)
            {
                foreach (JProperty property in messageJObject)
                {
                    string key = property.Name;
                    JToken value = property.Value;

                    if (key == "id") {
                        messageId = int.Parse(value.ToString());
                    }

                    else if (key == "name")
                    {
                        result = value.ToString() == EXPECTED_MESSAGE_NAME;
                    }

                    if (!result)
                        break;
                }
            }

            if (response.Count != 1)
                result = false;
            else { 
                var getMessageResponse = await v6client.GetMessage(messageId);

                foreach (JProperty property in getMessageResponse)
                {
                    string key = property.Name;
                    JToken value = property.Value;

                    if (key == "name") {
                        result = value.ToString() == EXPECTED_MESSAGE_NAME;
                    }
                    else if (key == "subject")
                    {
                        result = value.ToString() == EXPECTED_SUBJECT_NAME;
                    }
                    else if (key == "type") {

                        result = value.ToString() == EXPECTED_TYPE;
                    }

                    if (!result)
                        break;
                }

            }

            Assert.IsTrue(result);
        }


        [TestMethod]
        public async Task GetMessageContent()
        {
            bool result = true;
            string EXPECTED_MESSAGE_NAME = "A test Plain Message";

            string EXPECTED_MESSAGE_CONTENT_1 = "Welcome to your new template";
            string EXPECTED_MESSAGE_CONTENT_2 = "Click anywhere in your template to change the format and design. Double click text or images to add your content or hover to see other options. You can move things around and drag on new components from the content tab.";
            string EXPECTED_MESSAGE_CONTENT_3 = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit, sed diam nonummy nibh euismod tincidunt ut laoreet dolore magna aliquam erat.";

            var response = await v6client.GetMessages();
            int messageId = 0;

            foreach (JProperty property in response.First)
            {
                string key = property.Name;
                JToken value = property.Value;

                if (key == "id")
                {
                    messageId = int.Parse(value.ToString());
                }

                else if (key == "name")
                {
                    result = value.ToString() == EXPECTED_MESSAGE_NAME;
                }

                if (!result)
                    break;
            }

            if(result)
            {
                var getMessageResponse = await v6client.GetMessageContent(messageId);

                foreach (JProperty property in getMessageResponse)
                {
                    string key = property.Name;
                    JToken value = property.Value;

                    if (key == "body_text")
                    {
                        result = value.ToString().IndexOf(EXPECTED_MESSAGE_CONTENT_1) != -1;
                        result = value.ToString().IndexOf(EXPECTED_MESSAGE_CONTENT_2) != -1;
                    }
                    else if (key == "body_html")
                    {
                        result = value.ToString().IndexOf(EXPECTED_MESSAGE_CONTENT_3) != -1;
                    }

                    if (!result)
                        break;
                }

            }

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task TransactionalSend()
        {
            bool result = true;

            Transaction transactionContent = new Transaction()
            {

                list_id = 797032,
                type = "email",
                group_name = "Test group",
                subject = "Test Send",
                from_name = "Test", 
                from_address = "marketing@qubisoft.com.au",
                body_text = "test transcational group",
                recipients = new List<Recipient>
                {
                    new Recipient { email = "freddy@test.com"},
                }


            };


            var responseSend = await v6client.TransactionalSend(transactionContent);
            if ((int)responseSend.StatusCode == 202) {
                var responseGroups = await v6client.GetTransactionGroups();
                foreach (var group in responseGroups["_embedded"]["transactional-groups"])
                {
                    if (group["name"] == transactionContent.group_name && group["sent"] > 0)
                        result = true;

                    break;

                }
            }


            Assert.IsTrue(result);
        }
        // will need to at least run the above method for this to pass or create a transactional group some other way
        // The method above essentially does the same test as this
        [TestMethod]
        public async Task GetTransactionGroups()
        {
            bool result = false;

            var response = await v6client.GetTransactionGroups();

            // test object
            Transaction transactionContent = new Transaction()
            {
                list_id = 797032,
                type = "email",
                group_name = "Test group",
                subject = "Test Send",
                from_name = "Test",
                from_address = "marketing@qubisoft.com.au",
                body_text = "test transcational group",

                recipients = new List<Recipient>
                {
                    new Recipient { email = "freddy@test.com"},
                }
            };

            // the first group should match the test group that we have already created
            foreach (var group in response["_embedded"]["transactional-groups"])
            {
                if (group["name"] == transactionContent.group_name)
                    result = true;

                break;

            }


            Assert.IsTrue(result);
        }


    }
}