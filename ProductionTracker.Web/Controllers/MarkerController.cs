﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProductionTracker.Data;

namespace ProductionTracker.Web.Controllers
{
    public class MarkerController : Controller
    {
        // GET: Marker
        [Route("Markers")]
        public ActionResult Home()
        {
            return View();
        }
        public ActionResult GetMarkeCatsWithMarkers()
        {
            var repo = new MarkerRespository(Properties.Settings.Default.ManufacturingConStr);
            var item = repo.GetMarkerCategoriesWithAllMarkers().Select(mc =>
            {
                //var markerCat = mc.GetObjectBasePropertiesOnDbObject();
                return new
                {
                    mc.Id,
                    mc.Name,
                    mc.CreatedOn,
                    mc.BodyStyleId,
                    BodyStyle = new
                    {
                        mc.BodyStyle.Id,
                        mc.BodyStyle.Name
                    },
                    mc.DepartmentId,
                    Department = new
                    {
                        mc.Department.Id,
                        mc.Department.Name
                    },
                    mc.SleeveId,
                    Sleeve = new
                    {
                        mc.Sleeve.Id,
                        mc.Sleeve.Name
                    },
                    mc.ModifiedOn,
                    mc.DefaltMarkerId,
                    DefaltMarker = mc.Marker != null ? new
                    {
                        mc.Marker.Id,
                        mc.Marker.CreatedOn,
                        mc.Marker.ModifiedOn,
                        MarkerDetails = mc.Marker.MarkerDetails.Select(mmd =>
                        {
                            return new
                            {
                                mmd.Id,
                                mmd.SizeId,
                                mmd.AmountPerLayer,
                                Size = new
                                {
                                    mmd.Size.Id,
                                    mmd.Size.Name

                                },


                            };

                        }),
                        mc.Marker.Length,
                        mc.Marker.PercentWaste

                    } : null,
                    Markers = mc.Markers != null ?  mc.Markers.Select(m =>
                    {
                        return new
                        {
                            m.Id,
                            m.CreatedOn,
                            m.ModifiedOn,
                            m.Length,
                            m.PercentWaste,
                            MarkerDetails = m.MarkerDetails.Select(mmd =>
                            {
                                return new
                                {
                                    mmd.Id,
                                    mmd.SizeId,
                                    mmd.AmountPerLayer,
                                    Size = new
                                    {
                                        mmd.Size.Id,
                                        mmd.Size.Name

                                    },


                                };

                            })
                        };
                    }) : null
                } ;
                
                //markerCat.Marker = mc.Marker.GetObjectBasePropertiesOnDbObject();
                //markerCat.Marker.MarkerCategories = null;
                //markerCat.Markers = mc.Markers.Select(m =>
                //{
                //    var marker = m.GetObjectBasePropertiesOnDbObject();
                //    marker.MarkerCategories = null;
                //    marker.MarkerDetails = m.MarkerDetails.Select(md =>
                //    {
                //        var markerDetails = md.GetObjectBasePropertiesOnDbObject();
                //        markerDetails.Marker = null;
                //        markerDetails.Size = md.Size.GetObjectBasePropertiesOnDbObject();
                //        markerDetails.Size.MarkerDetails = null;
                //        return markerDetails;
                //    }).ToEntitySet();
                //    return marker;
                //}).ToEntitySet();
                //return markerCat;

            }).ToList();
            var x = item;
            return Json(item,JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public void UpdateMarker(Marker marker)
        {
            var repo = new MarkerRespository(Properties.Settings.Default.ManufacturingConStr);
            var prodRepo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            var originalMarker = repo.GetMarker(marker.Id);
            prodRepo.AddNewUpdateHistory(originalMarker, marker.Deleted ? "deleted": null );
            repo.UpdateMarker(originalMarker.SetOrginalDbObjToUpdated(marker));
        }
        [HttpPost]
        public void UpdateMarkerCat(MarkerCategory markerCategory)
        {
            var repo = new MarkerRespository(Properties.Settings.Default.ManufacturingConStr);
            var prodRepo = new ProductionRespository(Properties.Settings.Default.ManufacturingConStr);
            var originalMarker = repo.GetMarkerCategory(markerCategory.Id);
            prodRepo.AddNewUpdateHistory(originalMarker,markerCategory.Deleted ? "deleted" : null);
            repo.UpdateMarkerCat(originalMarker.SetOrginalDbObjToUpdated(markerCategory));
        }
        [HttpPost]
        public ActionResult AddNewMarkerCatGetMarker(MarkerCategory markerCategory, Marker defaltMarker)
        {
            AddNewMarkerCat(markerCategory, defaltMarker);
            return GetMarkerCategory(markerCategory.Id);
        }
        [HttpPost]
        public void AddNewMarkerCat(MarkerCategory markerCategory ,Marker defaltMarker)
        {
            var repo = new MarkerRespository(Properties.Settings.Default.ManufacturingConStr);
            repo.AddMarkerCat(markerCategory);
            if (defaltMarker.NotNull())
            {
                defaltMarker.MarkerCatId = markerCategory.Id;
                AddNewMarker(defaltMarker, true);
            }
        }
        [HttpPost]
        public void AddNewMarker(Marker marker,bool makeDefalt)
        {
            var repo = new MarkerRespository(Properties.Settings.Default.ManufacturingConStr);
            repo.AddMarker(marker);
            if (makeDefalt)
            {
                UpdateMarkerCat(new MarkerCategory
                {
                    Id = marker.MarkerCatId,
                    DefaltMarkerId = marker.Id
                });
            }
        }
        [HttpPost]
        public ActionResult AddNewMarkerGetResult(Marker marker, bool makeDefalt)
        {
            AddNewMarker(marker, makeDefalt);
            return GetMarkerCategory(marker.MarkerCatId);
        }

        public ActionResult GetMarkerCategory(int id)
        {
            var repo = new MarkerRespository(Properties.Settings.Default.ManufacturingConStr);
            var mc = repo.GetMarkerCategoryWithMarkers(id);
            return Json(new
            {
                mc.Id,
                mc.Name,
                mc.CreatedOn,
                mc.BodyStyleId,
                BodyStyle = new
                {
                    mc.BodyStyle.Id,
                    mc.BodyStyle.Name
                },
                mc.DepartmentId,
                Department = new
                {
                    mc.Department.Id,
                    mc.Department.Name
                },
                mc.SleeveId,
                Sleeve = new
                {
                    mc.Sleeve.Id,
                    mc.Sleeve.Name
                },
                mc.ModifiedOn,
                mc.DefaltMarkerId,
                DefaltMarker = mc.Marker != null ? new
                {
                    mc.Marker.Id,
                    mc.Marker.CreatedOn,
                    mc.Marker.ModifiedOn,
                    MarkerDetails = mc.Marker.MarkerDetails.Select(mmd =>
                    {
                        return new
                        {
                            mmd.Id,
                            mmd.SizeId,
                            mmd.AmountPerLayer,
                            Size = new
                            {
                                mmd.Size.Id,
                                mmd.Size.Name

                            },


                        };

                    }),
                    mc.Marker.Length,
                    mc.Marker.PercentWaste

                } :null,
                Markers = mc.Markers != null ? mc.Markers.Select(m =>
                {
                    return new
                    {
                        m.Id,
                        m.CreatedOn,
                        m.ModifiedOn,
                        m.Length,
                        m.PercentWaste,
                        MarkerDetails = m.MarkerDetails.Select(mmd =>
                        {
                            return new
                            {
                                mmd.Id,
                                mmd.SizeId,
                                mmd.AmountPerLayer,
                                Size = new
                                {
                                    mmd.Size.Id,
                                    mmd.Size.Name

                                },


                            };

                        })
                    };
                }):null
            }, JsonRequestBehavior.AllowGet);
        }
    }
}