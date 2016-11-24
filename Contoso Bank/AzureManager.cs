using Contoso_Bank.DataModels;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Contoso_Bank
{
    public class AzureManager
    {

        private static AzureManager instance;
        private MobileServiceClient client;
        private IMobileServiceTable<UserDatabase> userDatabaseTable;

        private AzureManager()
        {
            this.client = new MobileServiceClient("http://schocontosobank.azurewebsites.net");
            this.userDatabaseTable = this.client.GetTable<UserDatabase>();
        }

        public MobileServiceClient AzureClient
        {
            get { return client; }
        }

        public static AzureManager AzureManagerInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AzureManager();
                }

                return instance;
            }
        }
        public async Task AddUserDatabase(UserDatabase userDatabase)
        {
            await this.userDatabaseTable.InsertAsync(userDatabase);
        }
        public async Task<List<UserDatabase>> GetUserDatabase()
        {
            return await this.userDatabaseTable.ToListAsync();
        }

        public async Task UpdateUserDatabase(UserDatabase userDatabase)
        {
            await this.userDatabaseTable.UpdateAsync(userDatabase);
        }

        public async Task DeleteUserDatabase(UserDatabase userDatabase)
        {
            await this.userDatabaseTable.DeleteAsync(userDatabase);
        }
        
    }
}