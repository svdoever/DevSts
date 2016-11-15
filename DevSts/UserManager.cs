/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see LICENSE
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Web.Hosting;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace DevSts
{
    class User
    {
        public string Name { get; set; }
        public UserClaim[] Claims { get; set; }

    }

    class UserClaim
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }

    class UserManager
    {
        public static string[] GetAllUserNames()
        {
            var users = GetAllUsers();
            return users.Select(x => x.Name).Distinct().ToArray();
        }

        public static IEnumerable<Claim> GetClaimsForUser(string name)
        {
            var users = GetAllUsers();
            var user = users.FirstOrDefault(x => x.Name == name);
            if (user == null)
            {
                throw new Exception($"User: {name} is not found!");
            }
            var claims = user.Claims.Select(x => new Claim(x.Type, x.Value)).ToList();
            return claims;
        }

        public static IList<string> GetAllUniqueClaimTypes()
        {
            var users = GetAllUsers();            
            var claims = users.SelectMany(c => c.Claims).Select(o => o.Type).Distinct().ToList();

            return claims;
        } 

        static IEnumerable<User> GetAllUsers()
        {
            var ser = new JavaScriptSerializer();
            return (User[])ser.Deserialize(ReadClaimsFile(), typeof(User[]));
        }

        static string ClaimsFilePath
        {
            get
            {
                return Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "App_Data\\DevStsUsers.json");
            }
        }

        static string ReadClaimsFile()
        {
            using (var sw = File.OpenText(ClaimsFilePath))
            {
                return sw.ReadToEnd();
            }
        }

        internal static void WriteClaimsFile()
        {
            var path = ClaimsFilePath;
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (!File.Exists(path))
            {
                using (var sw = File.CreateText(path))
                {
                    var json = JsonConvert.SerializeObject(GetDefaultUsers(), Formatting.Indented);
                    sw.Write(json);
                    sw.Flush();
                }
            }
        }

        static User[] GetDefaultUsers()
        {
            return new User[]
            {
                new User
                {
                    Name = "Alice", 
                    Claims = new UserClaim[]
                    {
                        new UserClaim
                        {
                            Type = ClaimTypes.Name,
                            Value = "Alice"
                        },
                        new UserClaim
                        {
                            Type = ClaimTypes.Email,
                            Value = "alice@alice.com"
                        },
                        new UserClaim
                        {
                            Type = ClaimTypes.GivenName,
                            Value = "Alice"
                        },
                        new UserClaim
                        {
                            Type = ClaimTypes.Surname,
                            Value = "Doe"
                        },
                    }
                },
                new User
                {
                    Name = "Bob", 
                    Claims = new UserClaim[]
                    {
                        new UserClaim
                        {
                            Type = ClaimTypes.Name,
                            Value = "Bob"
                        },
                        new UserClaim
                        {
                            Type = ClaimTypes.Email,
                            Value = "bob@bob.com"
                        },
                        new UserClaim
                        {
                            Type = ClaimTypes.GivenName,
                            Value = "Bob"
                        },
                        new UserClaim
                        {
                            Type = ClaimTypes.Surname,
                            Value = "de Bouwer"
                        },
                    }
                }
            };
        }
    }
}
