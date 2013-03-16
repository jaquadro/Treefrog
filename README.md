Treefrog
========

A general purpose tile- and object-focused 2D level editor.  

Treefrog is still very much a work in progress and far from issue-free, but there's quite a bit of functionality currently available in the editor.  The current status of Treefrog's consituent projects are:

* Libs -- Pre-built dependencies for MonoGame, OpenTK, and LilyPath.
* Treefrog -- The main editor application.
* Treefrog.Framework -- Represents the core underlying data model and handles reading and writing project files.  Can be used in some game projects (e.g. I've been using it in Unity3D) to direclty load .TLP files, but this is far from optimal and should be used for testing and development only.
* Treefrog.Framework.Tests -- Some unit tests, not really maintained.  Ignore it.
* Treefrog.Pipeline -- An XNA pipeline extension for transforming Treefrog project files (.TLP) into a collection of optimized XNA-loadable assets.  This project is currently out of sync with the latest framework.
* Treefrog.Runtime -- The receiving end of Pipeline: Handles loading pipeline-generated assets, rendering, and providing an in-game data model.  This project is currently out of sync with the latest framework.
* TreefrogInstaller -- A WIX project for building an MSI installer.  You'll need to populate the Staging directory with the necessary assets first, like Treefrog.exe and supporting assemblies.
