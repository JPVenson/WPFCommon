using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Manager;

namespace DataAccess.QueryProvider
{
    public class TestQueryProvider : QueryProvider
    {
        public DbAccessLayer DbAccessLayer { get; set; }

        public TestQueryProvider(DbAccessLayer dbAccessLayer)
        {
            DbAccessLayer = dbAccessLayer;
        }

        #region Overrides of QueryProvider

        public override string GetQueryText(Expression expression)
        {
            return null;
        }

        public override object Execute(Expression expression)
        {
            return null;
        }

        #endregion
    }
}
