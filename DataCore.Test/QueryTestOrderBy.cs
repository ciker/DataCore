﻿using DataCore.Test.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataCore.Test
{
    [TestClass]
    public class QueryTestOrderBy
    {
        [TestMethod]
        public void CanGenerateOrderByWithDynamic()
        {
            var query = new Query<TestClass>(new Translator());

            query.OrderBy(t => new {t.Id, t.Name});

            Assert.AreEqual("TestClass.Id, TestClass.Name", query.SqlOrderBy);
        }

        [TestMethod]
        public void CanGenerateOrderByWithOneField()
        {
            var query = new Query<TestClass>(new Translator());

            query.OrderBy(t => t.Id);

            Assert.AreEqual("TestClass.Id", query.SqlOrderBy);
        }

        [TestMethod]
        public void CanAggregateOrderBy()
        {
            var query = new Query<TestClass>(new Translator());

            query.OrderBy(t => t.Id);
            query.OrderBy(t => new { t.Number, t.Name });

            Assert.AreEqual("TestClass.Id, TestClass.Number, TestClass.Name", query.SqlOrderBy);
        }
    }
}
