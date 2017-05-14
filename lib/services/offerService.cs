using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using iflix.models;
using iflix.providers;
using Newtonsoft.Json;

namespace iflix.services
{
    public class OfferService
    {
        private readonly Lazy<IFileProvider> _fileProvider;
        public OfferService(IFileProvider fileProvider)
        {
            _fileProvider = new Lazy<IFileProvider>(() => fileProvider);
        }

        public OfferService() : this(new FileProvider())
        {

        }

        public IDictionary<string, UserModel> LoadAccounts(string path)
        {
            AccountsModel accounts = null;
            var json = _fileProvider.Value.read(path);
            if (string.IsNullOrWhiteSpace(json))
            {
                // log ex
                Console.WriteLine($"accounts json {path} is empty");
                return null;
            }

            try
            {
                accounts = JsonConvert.DeserializeObject<AccountsModel>(json);
            }
            catch (Exception)
            {
                // log ex
                Console.WriteLine($"failed to deserialize accounts json {path}");
                return null;
            }
            return accounts?.users?.ToDictionary(c => c.number, c => c);
        }

        public IEnumerable<ActionModel> LoadActions(string path, string partner)
        {
            var json = _fileProvider.Value.read(path);
            ActionsModel actions = null;
            try
            {
                actions =  JsonConvert.DeserializeObject<ActionsModel>(json);
            }
            catch (Exception)
            {
                // log exception
                Console.WriteLine($"Failed to deserialize JSON for {partner}");                
            }       

            var list = new List<ActionModel>();

            if (actions?.revocations?.Any() == true)
            {
                list.AddRange(actions.revocations.Select(c => {
                    c.action = "revocation";
                    c.partner = partner;
                    return c;
                }));
            }
            if (actions?.grants?.Any() == true)
            {
                list.AddRange(actions.grants.Select(c => {
                    c.action = "grant";
                    c.partner = partner;
                    return c;
                }));
            }
            return list;
        }

        public void Revoke(UserModel account, ActionModel action)
        {
            Console.WriteLine($"Process REVOKE action by {action.partner} for {account.name}");
                
            // must be the current partner
            if (account.partner == action.partner)
            {
                // is the offer still active
                var uo = account.offers.LastOrDefault(c => c.partner == action.partner);
                if (uo != null)
                {
                    if (uo.endDate > action.date)
                    {
                        // set the offer to the date of revocation
                        uo.endDate = action.date;
                        Console.WriteLine($"Set endDate to {action.date}");
                    }
                }

                // revoking will free up the partner
                account.partner = string.Empty;     
            }
            else
            {
                Console.WriteLine($"{account.name} does not belong to {action.partner}");
            }
        }

        public void Grant(UserModel account, ActionModel action)
        {
            Console.WriteLine($"Process GRANT action by {action.partner} for {account.name}");
            if (!action.period.HasValue)
            {
                // should be ignored completely
                Console.WriteLine("no period defined. skipping...");
                return;
            }
            // only if partner is empty or same
            if (string.IsNullOrWhiteSpace(account.partner) || account.partner == action.partner)
            {
                account.partner = action.partner;
                var uo = account.offers.LastOrDefault(c => c.partner == action.partner);
                if (uo == null)
                {
                    uo = new UserOfferModel();
                    uo.startDate = action.date;
                    uo.endDate = action.date;
                    uo.partner = action.partner;
                    account.offers.Add(uo);
                }

                // add on to the endDate
                if (uo.endDate > action.date)
                {
                    Console.WriteLine($"Shifting start date from {action.date} to {uo.endDate}");
                    action.date = uo.endDate;                    
                }
             
                var newDate = action.date.AddMonths(action.period.Value);
                var diff = (newDate - action.date).TotalDays;
                uo.endDate = uo.endDate.AddDays(diff);       
                Console.WriteLine($"{diff} days added for {account.name} starting from {action.date}");
            }
            else
            {
                Console.WriteLine($"{account.name} does not belong to {action.partner}");
            }
        }

        public void ConstructOutput(IEnumerable<UserModel> accounts)
        {
            var eo = new ExpandoObject() as IDictionary<string,Object>;
            foreach (var account in accounts)
            {
                // assuming we only want to list accounts with offers
                if (!account?.offers?.Any() == true)
                {
                    continue;
                }

                var eo2 = new ExpandoObject() as IDictionary<string,Object>;
                
                foreach(var offer in account.offers.GroupBy(c => c.partner))
                {
                    var sum = offer.Sum(c => c.offer);
                    eo2.Add(offer.Key, sum);
                }
                eo.Add(account.name, eo2);
            }
            var subscriptions = new { subscriptions = eo};
            var result = JsonConvert.SerializeObject(subscriptions, Formatting.Indented);
            _fileProvider.Value.write(@"output/result.json", result);     
        }
        
    }
}