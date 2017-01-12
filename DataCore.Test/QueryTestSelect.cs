﻿using DataCore.Test.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataCore.Test
{
    [TestClass]
    public class QueryTestSelect
    {
        [TestMethod]
        public void DataTestSelectClauseNoWhere()
        {
            var data = new Query<TestClass>(new Translator());

            data.Build();

            Assert.AreEqual(data.SqlCommand.ToString(), "SELECT * FROM TestClass WITH(NOLOCK)");
        }

        [TestMethod]
        public void DataTestSelectClauseWhere()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id == 0).Build();

            Assert.AreEqual(data.SqlCommand.ToString(), "SELECT * FROM TestClass WITH(NOLOCK) WHERE (TestClass.Id = 0)");
        }

        [TestMethod]
        public void DataTestSelectClauseWithTop()
        {
            var data = new Query<TestClass>(new Translator());

            data.Top(10).Build();

            Assert.AreEqual(data.SqlCommand.ToString(), "SELECT TOP (10) * FROM TestClass WITH(NOLOCK)");
        }

        [TestMethod]
        public void DataTestSelectClauseWithTopAndWhere()
        {
            var data = new Query<TestClass>(new Translator());

            data.Where(t => t.Id == 0).Top(10).Build();

            Assert.AreEqual(data.SqlCommand.ToString(), "SELECT TOP (10) * FROM TestClass WITH(NOLOCK) WHERE (TestClass.Id = 0)");
        }

        [TestMethod]
        public void ComplexQueryTest()
        {
            var query =
                new Query<TestClass>(new Translator())
                    .Join<TestClass2>((t, t2) => t.Id == t2.Id)
                    .LeftJoin<TestClass2>((t, t2) => t.Id == t2.Id && t2.Id == 1)
                    .RightJoin<TestClass2, TestClass3>((t, t2) => t.Id == t2.Id && t2.Id > 1)
                    .Where(t => t.Number > 105)
                    .Top(103)
                    .Build();

            var expected = "SELECT TOP (103) * FROM TestClass WITH(NOLOCK)"
                            + " INNER JOIN TestClass2 WITH(NOLOCK) ON (TestClass.Id = TestClass2.Id)"
                            + " LEFT JOIN TestClass2 WITH(NOLOCK) ON ((TestClass.Id = TestClass2.Id) AND (TestClass2.Id = 1))"
                            + " RIGHT JOIN TestClass3 WITH(NOLOCK) ON ((TestClass2.Id = TestClass3.Id) AND (TestClass3.Id > 1))"
                            + " WHERE (TestClass.Number > 105)";

            Assert.AreEqual(expected, query.SqlCommand.ToString());
        }

        [TestMethod]
        public void CanGenerateSelectColumns()
        {
            var query = new Query<TestClass>(new Translator());
            query.Select(t => new { t.Id, t.Name }).Build();

            Assert.AreEqual("SELECT TestClass.Id, TestClass.Name FROM TestClass WITH(NOLOCK)", query.SqlCommand.ToString());
        }

        [TestMethod]
        public void CanGenerateSelectColumnsWithTop()
        {
            var query = new Query<TestClass>(new Translator());
            query.Top(10).Select(t => new { t.Id, t.Name }).Build();

            Assert.AreEqual("SELECT TOP (10) TestClass.Id, TestClass.Name FROM TestClass WITH(NOLOCK)", query.SqlCommand.ToString());
        }
    }
}
