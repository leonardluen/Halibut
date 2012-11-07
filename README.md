# Halibut

An **experimental** IDE with an emphasis on DCPU-16 programming. Halibut is a Windows-only project, sorry to everyone else.

## Usage

Make sure you have [Organic](https://github.com/SirCmpwn/Organic) in your path, then build Halibut.

Run Halibut.exe and play around with it.

## Project Files

Halibut project files are extremely simple and take the `.config` extension. They are a simple system of keys and values with
bash-style comments. Here's an example:

    # Example Project
    # Created on Tuesday, November 06, 2012
    
    # Project Name
    name=example
    # Build Command
    build=Organic.exe base.dasm bin/Boilerplate1.bin --json bin/example.json --listing bin/example.lst --include inc/
    # Error Regex (matched on build output)
    error-regex=Error (?<file>[A-Za-z0-9_ ./\-]+) \(line (?<line>[0-9]+)\): (?<error>.*)
    # Debug Command
    debug-exe=Lettuce.exe bin/example.bin --server --nogui --port 46852
    # Debug Port (for DCPU-16 assembly integrated debugging)
    debug=46852

## Templates

Templates are drawn from the built-in templates and from the user's `Documents/Halibut/Templates/` folder. You are encouraged
to create a folder for each template. Templates are represented with .template files, such as this:

    <?xml version="1.0" encoding="utf-8" ?>
    <project name="Boilerplate" icon="boilerplate.png">
      <file name="{project-name}.config" template="enviornment.config" project="true"></file>
      <file name="base.dasm" template="base.dasm" open="true"></file>
      <file name="inc/boilerplate.dasm" template="boilerplate.dasm"></file>
      <file name="{project-fname}.dasm" template="main.dasm" open="true" focus="true"></file>
    </project>

### Template XML

A number of files may be included in a template, at least one of which must have `project="true"`. The following attributes
are available on the file nodes:

* name: The file name to create in the new project
* template: The template file to use as the basis of the new file
* open: Set to true to open this file after project creation
* focus: Set to true to indicate this file should be the selected file after project creation

### Template Variables

Template variables take the form of `{variable name}`. They are run on file names and template contents while creating projects.
The following variables are available:

* `{project-name}`: The name of the project being created
* `{project-fname}`: The name of the project, with all invalid file characters removed and spaces removed
* `{current-date}`: The current date in the format `Weekday, Month DD, YYYY`

See the built-in templates for examples.

## Supported Languages

Template files are only provided for DCPU-16 assembly. All other languages should be theoretically supported if you configure
them manually, or through the (*unimplemented*) manual configuration wizard. However, only certain languages have syntax
highlighting support:

* DCPU-16 Assembly
* Markdown
* C/C++
* C#
* JavaScript
* HTML
* ASP/XHTML
* Boo
* Coco
* CSS
* Java
* Patch
* PowerShell
* PHP
* TeX
* VBNET
* XML

More languages may be added by plugins (*unimplemented*).

## Text Editor

Your standard coding text editor. All your favorite things like auto-indent and block select and such are included.