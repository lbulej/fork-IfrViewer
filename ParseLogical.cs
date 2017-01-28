﻿//MIT License
//
//Copyright(c) 2017-2017 Peter Kirmeier
//
//Permission Is hereby granted, free Of charge, to any person obtaining a copy
//of this software And associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, And/Or sell
//copies of the Software, And to permit persons to whom the Software Is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice And this permission notice shall be included In all
//copies Or substantial portions of the Software.
//
//THE SOFTWARE Is PROVIDED "AS IS", WITHOUT WARRANTY Of ANY KIND, EXPRESS Or
//IMPLIED, INCLUDING BUT Not LIMITED To THE WARRANTIES Of MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE And NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS Or COPYRIGHT HOLDERS BE LIABLE For ANY CLAIM, DAMAGES Or OTHER
//LIABILITY, WHETHER In AN ACTION Of CONTRACT, TORT Or OTHERWISE, ARISING FROM,
//OUT OF Or IN CONNECTION WITH THE SOFTWARE Or THE USE Or OTHER DEALINGS IN THE
//SOFTWARE.

using IFR;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using static IFR.IFRHelper;

namespace IfrViewer
{
    static class HpkParser
    {
        public struct ParsedHpkNode
        {
            public readonly HPKElement Origin;
            public string Name;
            public List<ParsedHpkNode> Childs;

            public ParsedHpkNode(HPKElement Origin, string Name)
            {
                this.Origin = Origin;
                this.Name = Name;
                Childs = new List<ParsedHpkNode>();
            }
        };


        public static List<ParsedHpkNode> ParseHpkPackages(List<HiiPackageBase> Packages)
        {
            List<ParsedHpkNode> result = new List<ParsedHpkNode>();

            foreach (HiiPackageBase pkg in Packages)
            {
                ParsedHpkNode root = new ParsedHpkNode(pkg, pkg.Name);

                try
                {
                    switch (pkg.PackageType)
                    {
                        case EFI_HII_PACKAGE_e.EFI_HII_PACKAGE_FORMS:
                            root.Name = "Form Package";
                            result.Add(root);
                            foreach (HiiIfrOpCode child in root.Origin.Childs)
                            {
                                ParsedHpkNode node = new ParsedHpkNode(child, child.Name);
                                root.Childs.Add(node);
                                ParsePackageIfr(child, node, Packages);
                            }
                            break;
                        default:
                            CreateLogEntryParser(LogSeverity.UNIMPLEMENTED, root.Name);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    CreateLogEntryParser(LogSeverity.ERROR, "Parsing failed!" + Environment.NewLine + ex.ToString());
                    MessageBox.Show("Parsing failed!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            return result;
        }

        private static void ParsePackageIfr(HiiIfrOpCode elem, ParsedHpkNode parent, List<HiiPackageBase> Packages)
        {
            // add all child elements to the tree..
            if (elem.Childs.Count > 0)
            {
                foreach (HiiIfrOpCode child in elem.Childs)
                {
                    ParsedHpkNode node = new ParsedHpkNode(child, child.Name);

                    switch (child.OpCode)
                    {
                        case EFI_IFR_OPCODE_e.EFI_IFR_END_OP: continue; // Skip
                        default: break; // simply add all others 1:1 when no specific handler exists
                    }

                    ParsePackageIfr(child, node, Packages);
                    parent.Childs.Add(node);
                }
            }
        }

        private static void CreateLogEntryParser(LogSeverity severity, string msg)
        {
            CreateLogEntry(severity, "Parser", msg);
        }

    }
}