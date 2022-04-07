﻿namespace gView.Framework.IO
{
    public interface IFileSystemDependent
    {
        bool FileChanged(string filename);
        bool FileDeleted(string filename);
    }
}
