using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard;
using Orchard.Events;

namespace NGM.Forum.Services.Sorting
{
    public interface ISortCriterionProvider : IEventHandler
    {
        IEnumerable<ISortCriterion> GetSortCriteria();
        ISortCriterion GetSortCriteria(string name);
    }

    public class SortCriterionProvider : ISortCriterionProvider
    {
        public IEnumerable<ISortCriterion> GetSortCriteria()
        {
            throw new NotImplementedException();
        }

        public ISortCriterion GetSortCriteria(string name)
        {
            throw new NotImplementedException();
        }
    }
}