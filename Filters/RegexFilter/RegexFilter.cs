//********************************************************************************************
//Author: Sergey Stoyan, CliverSoft.com
//        http://cliversoft.com
//        stoyan@cliversoft.com
//        sergey.stoyan@gmail.com
//        17 February 2008
//Copyright: (C) 2008, Sergey Stoyan
//********************************************************************************************

using System;
using System.Linq;
using System.Text.RegularExpressions;
using Cliver.DataSifter;

namespace Cliver
{
    internal class RegexFilter : Filter
    {
        override protected Version get_version()
        {
            return new Version(1, 0);
        }

        override protected string get_description()
        {
            return @"Based on .NET System.Text.RegularExpressions library.
Regex can be any supported by it. See the library's help for more information.
However, avoid embedded groups as their captures are confusing and displayed hardly. Use child regexes instead of embedded groups.";
        }

        override internal string HelpUrl
        {
            get
            {
                return "";
            }
        }

        public RegexFilter(Version version, string serialized_filter, string input_group_name, string comment)
            : base(version, input_group_name, comment)
        {
            if (serialized_filter == null)
                serialized_filter = get_default_serialized_filter();
            if (serialized_filter == null)
                serialized_filter = "\n";
            Match m = Regex.Match(serialized_filter, @"(?'Regex'.*)\n(?'RegexOptions'.*)", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
            if (!m.Success)
                throw new Exception("Filter definition could not be parsed:\n" + serialized_filter);
            RegexOptions options = RegexOptions.None;
            Enum.TryParse(m.Groups["RegexOptions"].Value, out options);
            Regex = new Regex(m.Groups["Regex"].Value, options);
            fill_group_index2raw_group_names();
        }

        internal Regex Regex;

        override public string[] GetGroupRawNames()
        {
            return Regex.GetGroupNames();
        }

        override public string GetSerializedFilter()
        {
            return Regex.ToString() + "\n" + Regex.Options.ToString();
        }

        override public FilterMatchCollection Matches(FilterGroup group)
        {
            return new RegexMatches(group, Regex);
        }

        override internal FilterControl CreateControl()
        {
            RegexControl c = new RegexControl(this);
            return c;
        }

        //internal override bool IsEqual(Filter filter)
        //{
        //    if (filter == null)
        //        return false;
        //    RegexFilter node = filter as RegexFilter;
        //    if (node == null)
        //        return false;
        //    if (InputGroupName == node.InputGroupName
        //        && Regex.ToString() == node.Regex.ToString()
        //        && Regex.Options == node.Regex.Options
        //        )
        //        return true;
        //    return false;
        //}  

        override internal string ReadableTypeName
        {
            get
            {
                return ".NET Regex";
            }
        }

        public override string GetVisibleDefinition()
        {
            return Regex.ToString();
        }
    }

    public class RegexMatches : FilterMatchCollection
    {
        public RegexMatches(FilterGroup parent_group, Regex regex)
            : base(parent_group)
        {
            m0 = regex.Match(parent_group.Text);
            if (!m0.Success)
                return;
            Reset();
        }
        readonly Match m0 = null;
        Match m;
        
        override public void Reset()
        {
            if (m0 == null)
                return;
            m = m0;
            reset = true;
        }
        bool reset = false;
        
        override public FilterMatch Current
        {
            get
            {
                if (m == null)
                    return null;
                return new FilterMatch((from x in m.Groups.Cast<Group>() select new FilterGroup(ParentGroup, x.Index, x.Value)).ToList<FilterGroup>());
            }
        }
        
        override public bool MoveNext()
        {
            if (m == null)
               return false;
            if (reset)
                reset = false;
            else
                m = m.NextMatch();
            return m.Success;
        }
    }

    //public class RegexFilterGroup : FilterGroup
    //{
    //}

    //public class StringParsableObject : IParsableObject
    //{
    //    public StringParsableObject(string text)
    //    {
    //        this.text = text;
    //    }
        
    //    readonly string text;

    //    public string ToString()
    //    {
    //        return text;
    //    }
    //}
}