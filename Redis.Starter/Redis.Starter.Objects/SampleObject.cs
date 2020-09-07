using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Redis.Starter.Objects
{
    public class SampleObjectList<T> : List<T>, ISerializable
    {
        public SampleObjectList()
        {
        }

        public SampleObjectList(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                return;

            //Title = info.GetString("Title");
            //Id = info.GetInt32("Id");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                return;

            //info.AddValue("Title", Title);
            //info.AddValue("Id", Id);
        }
    }

    [Serializable]
    [DataContract]
    public class SampleObject
    {
        [DataMember(Order = 1)]
        public string Title { get; set; }

        [DataMember(Order = 2)]
        public int Id { get; set; }
    }
}
