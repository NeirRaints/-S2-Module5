//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RegMovementInHospital
{
    using System;
    using System.Collections.Generic;
    
    public partial class Hospitalization
    {
        public int HospitalizationId { get; set; }
        public int PatientId { get; set; }
        public int HospitalizationCode { get; set; }
        public System.DateTime AppointedDate { get; set; }
        public string HospitalizationObj { get; set; }
        public string Department { get; set; }
        public Nullable<int> Conditions { get; set; }
        public Nullable<System.DateTime> DeadlineDate { get; set; }
        public string Additionally { get; set; }
    
        public virtual Patient Patient { get; set; }
    }
}
