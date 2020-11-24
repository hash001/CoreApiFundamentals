using AutoMapper;
using CoreCodeCamp.Models;

namespace CoreCodeCamp.Data
{
    public class CampProfile : Profile
    {
        public CampProfile()
        {
            this.CreateMap<Camp, CampModel>()
                .ForMember(c => c.Venue, o => o.MapFrom(m => m.Location.VenueName))
                .ForMember(c => c.Address1, o => o.MapFrom(m => m.Location.Address1))
                .ForMember(x => x.Address2, o => o.MapFrom(m => m.Location.Address2))
                .ForMember(c => c.Address3, o => o.MapFrom(m => m.Location.Address3))
                .ForMember(c => c.CityTown, o => o.MapFrom(m => m.Location.CityTown))
                .ForMember(c=>c.StateProvince, o=>o.MapFrom(m=>m.Location.CityTown))
                .ForMember(c => c.PostalCode, o=>o.MapFrom(m => m.Location.PostalCode))
                .ForMember(c=>c.Country, o => o.MapFrom(m=>m.Location.Country))
                .ReverseMap();

            this.CreateMap<Talk, TalkModel>()
                .ReverseMap()
                .ForMember(t => t.Camp, opt => opt.Ignore())
                .ForMember(t=>t.Speaker, opt => opt.Ignore());

            this.CreateMap<Speaker, SpeakerModel>()
                .ReverseMap();

            //this.CreateMap<CampModel, Camp>()
            //    .ForPath(c => c.Location.VenueName, o => o.MapFrom(m => m.Venue))
            //    .ForPath(c => c.Location.Address1, o => o.MapFrom(m => m.Address1))
            //    .ForPath(x => x.Location.Address2, o => o.MapFrom(m => m.Address2))
            //    .ForPath(c => c.Location.Address3, o => o.MapFrom(m => m.Address3))
            //    .ForPath(c => c.Location.CityTown, o => o.MapFrom(m => m.CityTown))
            //    .ForPath(c => c.Location.StateProvince, o => o.MapFrom(m => m.CityTown))
            //    .ForPath(c => c.Location.PostalCode, o => o.MapFrom(m => m.PostalCode))
            //    .ForPath(c => c.Location.Country, o => o.MapFrom(m => m.Country))
            //    .ReverseMap();
        }
    }
}
