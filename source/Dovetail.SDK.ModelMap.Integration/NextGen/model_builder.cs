using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.ModelMap.NextGen;
using FChoice.Common.Data;
using NUnit.Framework;

namespace Dovetail.SDK.ModelMap.Integration.NextGen
{
	public class CaseModelFilter
	{
		public string Id { get; set; }
	}

	public class CaseModel
	{
		public string Title { get; set; }
		public string Id { get; set; }
		public string SiteName { get; set; }
	}

	[TestFixture]
	public class model_builder_simple_scenario : MapFixture
	{
		private IEnumerable<CaseModel> _results;
		private CaseDTO _case;

		public override void beforeAll()
		{
			var objectMother = new ObjectMother(AdministratorClarifySession);

			_case = objectMother.CreateCase();

			var mapFactory = Container.GetInstance<IModelMapConfigFactory<CaseModelFilter, CaseModel>>();

			var map = mapFactory.Create("case", c =>
				{
					c.SelectField("id_number", s => s.Id).EqualTo(f=>f.Id);
					c.SelectField("title", s => s.Title);
					c.Join("case_reporter2site", site=>
						{
							site.SelectField("name", s => s.SiteName);
						});
				});
			Container.Configure(c => c.For<IModelMapConfig<CaseModelFilter, CaseModel>>().Use(map));

			var modelBuilder = Container.GetInstance<IModelBuilder<CaseModelFilter, CaseModel>>();

			var filter = new CaseModelFilter {Id = _case.IDNumber};

			_results = modelBuilder.Execute(filter);
		}

		[Test]
		public void found_id()
		{
			_results.First().Id.ShouldEqual(_case.IDNumber);
		}

		[Test]
		public void found_title()
		{
			_results.First().Title.ShouldEqual(_case.Title);
		}

		[Test]
		public void found_site_name_via_join()
		{
			_results.First().SiteName.ShouldEqual(_case.Site.Name);
		}
	}

	[TestFixture]
	public class model_builder_filterable: MapFixture
	{
		private IModelMapConfig<ModemModel, ModemModel> _map;
		private IModelBuilder<ModemModel, ModemModel> _modelBuilder;
		private int _modemCount;
		private ModemModel _modemModel;

		public override void beforeAll()
		{
			var mapFactory = Container.GetInstance<IModelMapConfigFactory<ModemModel, ModemModel>>();

			_map = mapFactory.Create("modem", c =>
				{
					c.SelectField("objid", s => s.ObjId).FilterableBy(f => f.ObjId);
					c.SelectField("hostname", s => s.HostName).FilterableBy(f => f.HostName);
					c.SelectField("device_name", s => s.DeviceName).FilterableBy(f=>f.DeviceName);
				});

			Container.Configure(c => c.For<IModelMapConfig<ModemModel, ModemModel>>().Use(_map));

			_modelBuilder = Container.GetInstance<IModelBuilder<ModemModel, ModemModel>>();

			SqlHelper.ExecuteNonQuery("DELETE FROM table_modem");

			_modemModel = CreateModem();
			CreateModem();
			CreateModem();

			_modemCount = Convert.ToInt32(SqlHelper.ExecuteScalar("SELECT COUNT(*) FROM table_modem"));
		}

		private ModemModel CreateModem()
		{
			var modemGeneric = AdministratorClarifySession.CreateDataSet().CreateGeneric("modem");
			var modem = modemGeneric.AddNew();
	
			var hostName = Guid.NewGuid().ToString();
			var deviceName = Guid.NewGuid().ToString().Substring(0, 20);

			modem["hostname"] = hostName;
			modem["device_name"] = deviceName;
			modem["active"] = "active";
			modemGeneric.UpdateAll();

			return new ModemModel
				{
					ObjId = Convert.ToInt32(modem.UniqueID),
					HostName = hostName,
					DeviceName = deviceName
				};
		}

		[Test]
		public void should_have_no_constraint_when_filterable_is_not_set()
		{
			var filter = new ModemModel { ObjId = 1234 };

			var results = _modelBuilder.Execute(filter);

			results.Count().ShouldEqual(_modemCount);
		}

		[Test]
		public void should_use_filter_set_on_map()
		{
			var filter = new ModemModel { ObjId = _modemModel.ObjId };

			_map.SetFilter(f=>f.ObjId).Operator = new EqualsFilterOperator();

			var results = _modelBuilder.Execute(filter);

			results.First().Equals(_modemModel).ShouldBeTrue();
		}

/*		[Test]
		public void should_all_filters_set_on_map()
		{
			_map.SetFilter(f => f.ObjId).Operator = new EqualsFilterOperator();
			_map.SetFilter(f => f.HostName).Operator = new EqualsFilterOperator();
			_map.SetFilter(f => f.DeviceName).Operator = new EqualsFilterOperator();

			var results = _modelBuilder.Execute(_modemModel);

			results.First().Equals(_modemModel).ShouldBeTrue();
		}*/
	}

	public class ModemModel
	{
		public int ObjId { get; set; }
		public string HostName { get; set; }
		public string DeviceName { get; set; }
		public string Active { get; set; }

		protected bool Equals(ModemModel other)
		{
			return ObjId == other.ObjId && String.Equals(DeviceName, other.DeviceName) && String.Equals(HostName, other.HostName);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((ModemModel)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = ObjId;
				hashCode = (hashCode * 397) ^ (DeviceName != null ? DeviceName.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (HostName != null ? HostName.GetHashCode() : 0);
				return hashCode;
			}
		}
	}
}