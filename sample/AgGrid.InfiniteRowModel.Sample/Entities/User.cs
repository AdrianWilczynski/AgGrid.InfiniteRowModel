using System;

namespace AgGrid.InfiniteRowModel.Sample.Entities
{
    public class User
    {
        public virtual int Id { get; set; }
        public virtual string FullName { get; set; }
        public virtual DateTime RegisteredOn { get; set; }
        public virtual int Age { get; set; }
        public virtual bool IsVerified { get; set; }
    }
}
