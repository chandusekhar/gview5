using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace gView.Framework.IO
{
	public interface IPersistStream 
	{
		object Load(string key);
		object Load(string key,object defVal);
		object Load(string key,object defVal,object objectInstance);

        Task<T> LoadAsync<T>(string key, T objectInstance, T defaultValue = default(T))
            where T : IPersistableLoadAsync;

        Task<T> LoadPluginAsync<T>(string key, T unknownPlugin=default(T))
            where T : IPersistableLoadAsync;

		void Save(string key,object val);
        void SaveEncrypted(string key, string val);
        //void Save(string key, object val, object objectInstance);
		//void WriteStream(string path);
		//void ReadStream(string path);
	}

    public interface IPersistable 
	{
        void Load(IPersistStream stream);
		void Save(IPersistStream stream);
	}

    public interface IPersistableLoadAsync
    {
        Task<bool> LoadAsync(IPersistStream stream);
        void Save(IPersistStream stream);
    }

    public interface IPersistableDictionary : IPersistable
    {
        object this[string key] { get; set; }
    }

    public interface IPersistableTemporaryRestore
    {
        void TemporaryRestore();
        void RemoveTemporeryRestore();
    }

    public interface IXmlString
    {
        string ToXmlString();
        void FromXmlString(string xml); 
    }

    public interface IBase64String
    {
        string ToBase64String();
        void FromBase64String(string base64);
    }

    /*
    public interface IPersistable2
    {
        void Load(object obj, IPersistStream stream);
        void Save(object obj, IPersistStream stream);
    }
*/

    public interface IMetadata
    {
        void ReadMetadata(IPersistStream stream);
        Task WriteMetadata(IPersistStream stream);

        IMetadataProvider MetadataProvider(Guid guid);
        Task<List<IMetadataProvider>> GetProviders();
    }

    

    public interface IMetadataObjectParameter
    {
        object MetadataObject { get; set; }
    }

    public interface IFileSystemDependent
    {
        bool FileChanged(string filename);
        bool FileDeleted(string filename);
    }

    public interface IFileWatchingDirectories
    {
        /// <summary>
        /// Gets the directories.
        /// </summary>
        /// <value>The directories.</value>
        List<DirectoryInfo> Directories { get; }

        /// <summary>
        /// Adds the directory.
        /// </summary>
        /// <param name="di">The di.</param>
        void AddDirectory(DirectoryInfo di);

        /// <summary>
        /// Removes the directory.
        /// </summary>
        /// <param name="di">The di.</param>
        void RemoveDirectory(DirectoryInfo di);
    }
}
