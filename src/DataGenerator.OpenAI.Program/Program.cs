﻿namespace DataGenerator.OpenAI.Program
{
    using DataGenerator.OpenAI.Program.Data.Database;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using DataGenerator.OpenAI.Program.Data.Entities;
    using DataGenerator.OpenAI.Program.Helpers;
    using System.Collections.Generic;
    using DataGenerator.OpenAI.Mock.Data.Generators;
    using DataGenerator.OpenAI.Data.Generators;
    using System.Globalization;

    class Program
    {
        static async Task Main(string[] args)
        {
            Random random = new Random();
            var connStr = Environment.GetEnvironmentVariable("LOCALHOST_MYSQL")!;
            var dbOptions = new DbContextOptionsBuilder<Context>().UseMySql(connStr, ServerVersion.AutoDetect(connStr),
                                mySqlOptionsAction: (MySqlDbContextOptionsBuilder sqlOptions) =>
                                {
                                    sqlOptions.EnableRetryOnFailure(
                                    maxRetryCount: 10,
                                    maxRetryDelay: TimeSpan.FromSeconds(30),
                                    errorNumbersToAdd: null);
                                    sqlOptions.CommandTimeout(240);
                                }).ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning)).Options;
            var context = new Context(dbOptions);
            var trace = new ConsoleTraceWriter();
            var openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!;
            string locale = CultureInfo.CurrentCulture.Name;

            try
            {
                MockDataGenerator mockDataGenerator = new MockDataGenerator(trace, openAiApiKey);
                EntityFrameworkDataGenerator<Context> entityFrameworkDataGenerator = new EntityFrameworkDataGenerator<Context>(context, mockDataGenerator, trace);

                var users = await entityFrameworkDataGenerator.GenerateData<User>(locale, 5, 5);

                context.Users.AddRange(users!);

                var schools = await entityFrameworkDataGenerator.GenerateData<School>(locale, 1, 1);

                foreach (var school in schools!)
                {
                    school.CreatedBy = users?[random.Next(0, users.Count() - 1)];
                    school.UpdatedBy = users?[random.Next(0, users.Count() - 1)];
                }

                context.Schools.AddRange(schools!);

                var schoolBranches = await entityFrameworkDataGenerator.GenerateData<SchoolBranch>(locale, 5, 5, schools?[0].SchoolName!);

                foreach (var schoolBranch in schoolBranches!)
                {
                    schoolBranch.School = schools?[0];
                    schoolBranch.CreatedBy = users?[random.Next(0, users.Count() - 1)];
                    schoolBranch.UpdatedBy = users?[random.Next(0, users.Count() - 1)];
                }

                context.SchoolBranches.AddRange(schoolBranches!);

                var countries = await entityFrameworkDataGenerator.GenerateData<Country>(locale, 1, 1);

                foreach (var country in countries!)
                {
                    country.SchoolBranch = schoolBranches?[random.Next(0, schoolBranches.Count() - 1)];
                    country.CreatedBy = users?[random.Next(0, users.Count() - 1)];
                    country.UpdatedBy = users?[random.Next(0, users.Count() - 1)];
                }

                context.Countries.AddRange(countries!);

                var states = await entityFrameworkDataGenerator.GenerateData<State>(locale, 25, 25);

                foreach (var state in states!)
                {
                    state.Country = countries?[0];
                    state.CreatedBy = users?[random.Next(0, users.Count() - 1)];
                    state.UpdatedBy = users?[random.Next(0, users.Count() - 1)];
                }

                context.States.AddRange(states!);

                var cities = new List<City>();

                foreach (var state in states!)
                {
                    var citiesInState = await entityFrameworkDataGenerator.GenerateData<City>(locale, 25, 25, state.Name!);

                    citiesInState?.ForEach((city) =>
                    {
                        city.State = state;
                        city.CreatedBy = users?[random.Next(0, users.Count() - 1)];
                        city.UpdatedBy = users?[random.Next(0, users.Count() - 1)];
                    });
                    cities.AddRange(citiesInState!);
                }

                context.Cities.AddRange(cities!);

                var addressTypes = await entityFrameworkDataGenerator.GenerateData<AddressType>(locale, 2, 2);

                foreach (var addressType in addressTypes!)
                {
                    addressType.SchoolBranch = schoolBranches?[random.Next(0, schoolBranches.Count() - 1)];
                    addressType.CreatedBy = users?[random.Next(0, users.Count() - 1)];
                    addressType.UpdatedBy = users?[random.Next(0, users.Count() - 1)];
                }

                context.AddressTypes.AddRange(addressTypes!);

                var addresses = new List<Address>();

                foreach (var city in cities!)
                {
                    var addressesInCity = await entityFrameworkDataGenerator.GenerateData<Address>(locale, 125, 125, city.Name!);

                    foreach (var address in addressesInCity!)
                    {
                        address.AddressType = addressTypes?[random.Next(0, addressTypes.Count() - 1)];
                        address.City = city;
                        address.CreatedBy = users?[random.Next(0, users.Count() - 1)];
                        address.UpdatedBy = users?[random.Next(0, users.Count() - 1)];
                    }

                    addresses.AddRange(addressesInCity);
                }

                context.Addresses.AddRange(addresses!);

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                trace.Log(ex.Message);
            }
        }
    }
}