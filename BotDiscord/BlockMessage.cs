using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotDiscord
{
    internal class BlockMessage : PagedMessage<string>
    {
        public override void BuildPages()
        {
            throw new NotImplementedException();
        }

        public void AddPage(string message)
        {
            Pages.Add(Pages.Count+1, () => 
            { 
                return message; 
            });
        }
    }
}
