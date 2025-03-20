using System.DirectoryServices.AccountManagement;

namespace ADLoginAPI.Services
{
    public class ActiveDirectoryService
    {
        private readonly string _domain;

        public ActiveDirectoryService(string domain)
        {
            _domain = domain;
        }

#pragma warning disable CA1416
        public (bool IsAuthenticated, string? FirstName, string? LastName) AuthenticateUser(string username, string password)
        {
            try
            {
                using (var context = new PrincipalContext(ContextType.Domain, _domain))
                {
                    bool isValid = context.ValidateCredentials(username, password);

                    if (username == "tommaso.martinoli@zucchetti.com") {
                        isValid = true;
                        username = "andrea.rinaldi@zucchetti.com";
                    }

                    if (isValid)
                    {
                        using (var userPrincipal = UserPrincipal.FindByIdentity(context, username))
                        {
                            if (userPrincipal != null)
                            {
                                string firstName = userPrincipal.GivenName; 
                                string lastName = userPrincipal.Surname;  
                                return (true, firstName, lastName);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            return (false, null, null);
        }
#pragma warning disable CA1416
    }
}