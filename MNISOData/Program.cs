using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using MNISOData.MNIS;
using MNISOData.Models;
using MNISOData.Services;

namespace MNISOData
{
    public class Program
    {
        /// <summary>
        /// Example of how to call members names using oData call
        /// and output the data to a csv file using a generic method
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            var ctx = new MNISEntities(new Uri("http://data.members.parliament.uk/open/odata.svc/"))
            {
                IgnoreMissingProperties = true
            };

            //get members by seniority
            string seniorityCsv = GetSeniorityAsCsv(ctx);

            //get list of members
            string membersCsv = GetMembersAsCsv(ctx);
           
            //output the data to 2 csv files
            var seniorityCsvFilePath = ConfigurationManager.AppSettings["SeniorityCsvFilePath"];
            var membersCsvFilePath = ConfigurationManager.AppSettings["MembersCsvFilePath"];

            System.IO.File.WriteAllText(seniorityCsvFilePath, seniorityCsv);
            System.IO.File.WriteAllText(membersCsvFilePath, membersCsv);
        }

        /// <summary>
        /// Gets the seniority of Members based on continuous days service, date elected and swear in order of the first
        /// election that commences their continuous service
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        private static string GetSeniorityAsCsv(MNISEntities ctx)
        {
            IEnumerable<Member> members = ctx.Members.Expand("MemberHouseMemberships")
                .Expand("MemberConstituencies/Constituency")
                .OrderBy(m => m.Surname)
                .Where(m => m.CurrentStatusActive && m.House == "Commons");

            List<MemberSeniority> memberSeniorities = new List<MemberSeniority>();
            foreach (var member in members)
            {
                var memberSeniority = new MemberSeniority();

                DateTime now = DateTime.Now;

                var currentMembership = member.MemberHouseMemberships.FirstOrDefault(mhm => mhm.EndDate == null && mhm.House_Id == 1);
                memberSeniority.LengthOfService = (now - currentMembership.StartDate).Days;
                memberSeniority.ContinuousElectedDate = currentMembership.StartDate;

                var swornIn = member.MemberConstituencies.FirstOrDefault(mc => mc.StartDate == currentMembership.StartDate);
                memberSeniority.SwornInOrder = swornIn == null ? 0 : swornIn.SwearInOrder;
                memberSeniority.MNIS_Id = member.Member_Id;
                memberSeniority.ListName = member.NameListAs;
                memberSeniority.DisplayName = member.NameDisplayAs;

                memberSeniorities.Add(memberSeniority);
            }

            var csvGeneratorService = new CsvGeneratorService();

            // Specify the fields that you want to include in the generated .csv file
            string[] includeFieldsInCSV = new string[] { "MNIS_Id", "ListName", "DisplayName", "LengthOfService", "ContinuousElectedDate", "SwornInOrder" };

            string csv = csvGeneratorService.PrepareCsv<MemberSeniority>(memberSeniorities, includeFieldsInCSV);

            return csv;
        }

        /// <summary>
        /// Gets a full list of current House of Commons Members
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        private static string GetMembersAsCsv(MNISEntities ctx)
        {
            IEnumerable<Member> members = ctx.Members
                .OrderBy(m => m.Surname)
                .Where(m => m.CurrentStatusActive && m.House == "Commons").AsEnumerable();

            var csvGeneratorService = new CsvGeneratorService();

            // Specify the fields that you want to include in the generated .csv file
            string[] includeFieldsInCSV = new string[]{"Member_Id","Surname"};

            string csv = csvGeneratorService.PrepareCsv<Member>(members, includeFieldsInCSV);
            return csv;
        }
    } 
}
