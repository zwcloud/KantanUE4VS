﻿using EnvDTE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KUE4VS
{
    public class AddTypeTask : AddCodeElementTask
    {
        public AddableTypeVariant Variant { get; set; }
        public string Base { get; set; }
        public SourceRelativeLocation Location { get; set; }
        public bool bPrivateHeader { get; set; }

        // Valid only for non-reflected types
        public string Namespace { get; set; }

        public AddTypeTask()
        {
            Location = new SourceRelativeLocation();
        }

        public override IEnumerable<GenericFileAdditionTask> GenerateAdditions(Project proj)
        {
            List<GenericFileAdditionTask> additions = new List<GenericFileAdditionTask>();

            string file_title = ElementName;
            string file_header = "/** Some copyright stuff. */";
            string type_keyword = Constants.TypeKeywords[Variant];
            bool is_reflected = Constants.ReflectedTypes[Variant];
            // @NOTE: Weirdly, passing null seems to crash the template processer
            string base_class = String.IsNullOrEmpty(Base) ? String.Empty : Base;
            string type_name = Utils.GetPrefixedTypeName(ElementName, base_class, Variant);
            bool should_export = false;

            // Header file
            {
                // Generate content

                List<string> default_includes = new List<string>{
                    "CoreMinimal.h"
                };

                string hdr_contents = CodeGeneration.SourceGenerator.GenerateTypeHeader(
                    file_title,
                    file_header,
                    default_includes,
                    type_keyword,
                    is_reflected,
                    type_name,
                    base_class,
                    Location.ModuleName,
                    should_export
                    );
                if (hdr_contents == null)
                {
                    return null;
                }

                // Now generate the paths where we want to add the files
                var hdr_folder_path = Utils.GenerateSubfolderPath(
                    proj,
                    Location.ModuleName,
                    bPrivateHeader ? ModuleFileLocationType.Private : ModuleFileLocationType.Public,
                    Location.RelativePath
                    // todo: plugin
                    );

                additions.Add(new GenericFileAdditionTask
                {
                    FileTitle = file_title,
                    Extension = ".h",
                    FolderPath = hdr_folder_path,
                    Contents = hdr_contents
                });
            }

            // Cpp file
            {
                // Generate content

                List<string> default_includes = new List<string>{
                };

                string cpp_contents = CodeGeneration.SourceGenerator.GenerateTypeCpp(
                    file_title,
                    file_header,
                    default_includes,
                    type_name
                    );
                if (cpp_contents == null)
                {
                    return null;
                }

                var cpp_folder_path = Utils.GenerateSubfolderPath(
                    proj,
                    Location.ModuleName,
                    ModuleFileLocationType.Private,
                    Location.RelativePath
                    // todo: plugin
                    );

                additions.Add(new GenericFileAdditionTask
                {
                    FileTitle = file_title,
                    Extension = ".cpp",
                    FolderPath = cpp_folder_path,
                    Contents = cpp_contents
                });
            }

            return additions;
        }
    }
}
