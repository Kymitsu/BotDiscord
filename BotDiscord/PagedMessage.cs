using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotDiscord
{
    public abstract class PagedMessage<T>
    {
        public int CurrentPage { get; set; }
        public Dictionary<int, Func<T>> Pages { get; set; }
        public List<ulong> MessageIds { get; set; }

        public PagedMessage()
        {
            CurrentPage = 1;
            Pages = new Dictionary<int, Func<T>>();
            MessageIds = new List<ulong>();
        }

        public abstract void BuildPages();

        public T GetCurrentPage()
        {
            return Pages[CurrentPage].Invoke();
        }
        public T GetNextPage()
        {
            if (CurrentPage < Pages.Count)
                CurrentPage++;
            else
                CurrentPage = 1;

            return Pages[CurrentPage].Invoke();
        }

        public T GetPreviousPage()
        {
            if (CurrentPage != 1)
                CurrentPage--;
            else
                CurrentPage = Pages.Count;

            return Pages[CurrentPage].Invoke();
        }
    }
}
