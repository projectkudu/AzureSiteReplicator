using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureSiteReplicator.Test
{
    public class TestHelpers
    {
        public static void VerifyEnumerable<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            IEnumerator<T> iterator1 = expected.GetEnumerator();
            IEnumerator<T> iterator2 = actual.GetEnumerator();
            bool moveNext1 = iterator1.MoveNext();
            bool moveNext2 = iterator2.MoveNext();

            while (moveNext1 && moveNext2)
            {
                if (!iterator1.Current.Equals(iterator2.Current))
                {
                    Assert.Fail(
                        "IEnumerables are not equal.  {0} != {1}",
                        iterator1.Current.ToString(),
                        iterator2.Current.ToString());
                    return;
                }

                moveNext1 = iterator1.MoveNext();
                moveNext2 = iterator2.MoveNext();
            }

            if (iterator1.Current != null || iterator2.Current != null)
            {
                Assert.Fail("IEnumerables are not equal because the collections lengths are not the same");
            }
        }

    }
}
