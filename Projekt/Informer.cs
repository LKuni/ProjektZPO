using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
//using NUnit.Framework;
using System.Text.RegularExpressions;

namespace Projekt
{
    public class Informer
    {
        Regex UrlMatch = new Regex(@"(?i)(http(s)?:\/\/)?(\w{2,25}\.)+\w{3}([a-z0-9\-?=$-_.+!*()]+)(?i)", RegexOptions.Singleline);


        public List<Url> urls = new List<Url>();
        public Boolean AddUrl(String title, String description)
        {
            if (title.Trim() == "" || description.Trim() == "" || UrlMatch.IsMatch(description) == false)
                return false;

            urls.Insert(0,new Url(title, description));

            return true;
        }

        public void RemoveUrl(Url url)
        {
            urls.Remove(url);


            
        }

        public String ShowUrlDetails(Url url)
        {
            return url.ToString();
        }
    }


    public class Url
    {
        public string Title { get; private set; }
        public string Description { get; private set; }
        public DateTime Created;
        
        public DateTime GetTime()
        {
            return DateTime.Now.Date;
        }
 

        public Url(string title, string desc)
        {
            Title = title;
            Description = desc;
            this.Created = DateTime.Now;
        }

        public override string ToString()
        {
            string shortDesc = Description;
            

            if (Description.Length > 20)
                 shortDesc = Description.Substring(0, 20) + "...";

            return string.Format("{0}: {1} at {2:yyyy.MM.dd}", Title, shortDesc, this.Created);
        }

    }
    


}
