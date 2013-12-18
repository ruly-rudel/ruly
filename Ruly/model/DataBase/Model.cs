using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SQLite;
using Android.Util;
using System.Linq;
using System.IO;
using System.Text;

namespace Ruly.model
{
	public class Model
	{
		// Model Data
		private ObservableCollection<TaskData> m_tasks;
		private ObservableCollection<CombinedTaskHistory> m_histories;
		private ObservableCollection<CombinedTaskHistory> m_raw_histories;
		private TaskCategory[] m_categories;
		private TaskCategoryRelation[] m_category_relations;
		private PersistentModelData m_data;
		private SQLiteConnection m_db;
		private TaskData m_unknown_task;

		public Model ()
		{
			// prepare model state/data
			m_data = new PersistentModelData ();

			// prepare DB
			string folder = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			m_db = new SQLiteConnection (System.IO.Path.Combine (folder, "ruly.db"));
			m_db.CreateTable<TaskData>();
			m_db.CreateTable<TaskHistory>();
			m_db.CreateTable<TaskAlarm> ();

			m_unknown_task = new TaskData () {
				Id = -1,
				Title = "休憩"
			};

			// create ViewModel I/F datasets
			Tasks = new ObservableCollection<TaskData>();
			TaskHistories = new ObservableCollection<CombinedTaskHistory> ();
			RawTaskHistories = new ObservableCollection<CombinedTaskHistory> ();
			UpdateTaskList ();
			UpdateTaskHistoryList ();
		}

		public ObservableCollection<TaskData> Tasks {
			get {
				return m_tasks;
			}
			private set {
				m_tasks = value;
			}
		}

		public ObservableCollection<CombinedTaskHistory> TaskHistories {
			get {
				return m_histories;
			}
			private set {
				m_histories = value;
			}
		}

		public ObservableCollection<CombinedTaskHistory> RawTaskHistories {
			get {
				return m_raw_histories;
			}
			private set {
				m_raw_histories = value;
			}
		}

		public IEnumerable<TaskAlarm> CurrentAlarms {
			get {
				var alarms = from x in m_db.Table<TaskAlarm> () select x;

				var ret = new TaskAlarm[alarms.Count ()];
				int i = 0;
				foreach (var x in alarms) {
					ret [i] = new TaskAlarm ();
					ret[i].AlarmDateTime = x.AlarmDateTime.Add(DateTime.Today - new DateTime (1970, 1, 1));
					if ((ret[i].AlarmDateTime - DateTime.Now.AddSeconds (10)).TotalSeconds <= 0.0) {
						ret[i].AlarmDateTime = ret[i].AlarmDateTime.AddDays (1.0);
					}
					ret [i].Id = x.Id;
					ret [i].TaskId = x.TaskId;
					i++;
				}

				return ret;
			}
		}

		public TaskCategory[] TaskCategories {
			get {
				return m_categories;
			}
			private set {
				m_categories = value;
			}
		}


		public TaskCategoryRelation[] TaskCategoryRelations {
			get {
				return m_category_relations;
			}

			private set {
				m_category_relations = value;
			}
		}

		public int State {
			get { 
				return m_data.state;
			}
			private set { 
				m_data.state = value;
			}
		}

		public TaskData GetTaskFromId (int taskId)
		{
			if (taskId == -1) {
				return m_unknown_task;
			} else {
				var q = from x in m_tasks
					    where x.Id == taskId
				    select x;

				if (q.Count() == 1) {
					return q.First();
				} else {
					return null;
				}
			}
		}

		public IEnumerable<TaskAlarm> GetAlarmFromTaskId (int id)
		{
			return from x in m_db.Table<TaskAlarm> ()
			       where x.TaskId == id
			       select x;
		}

		public TaskAlarm GetAlarmFromId (int id)
		{
			return (from x in m_db.Table<TaskAlarm> ()
				where x.Id == id
				select x).First();
		}

		public void Insert (TaskData task)
		{
			m_db.Insert (task);
			UpdateTaskList ();
		}

		public void Update (TaskData task)
		{
			m_db.Update (task);
			UpdateTaskList ();
		}

		public void Insert (TaskHistory task)
		{
			m_db.Insert (task);
			UpdateTaskHistoryList ();
		}

		public void Update (TaskHistory task)
		{
			m_db.Update (task);
			UpdateTaskHistoryList ();
		}

		public void Insert (TaskAlarm alarm)
		{
			m_db.Insert (alarm);
		}

		public void Update (TaskAlarm alarm)
		{
			m_db.Update (alarm);
		}

		public void Delete (TaskAlarm alarm)
		{
			m_db.Delete<TaskAlarm> (alarm.Id);
		}

		public void Update (CombinedTaskHistory task)
		{
			if (task.Id == -1) {	// not in the DB
				m_db.Insert (new TaskHistory () {
					TaskId = task.TaskId [0],
					Begin = task.Begin,
					End = task.End
				});
//			} else if (task.Id != -1 && task.TaskId[0] == -1) {	// to rest
//				m_db.Delete (task.Id);
			} else {
				m_db.Update (new TaskHistory () {
					Id = task.Id,
					TaskId = task.TaskId[0],
					Begin = task.Begin,
					End = task.End
				});
			}
			UpdateTaskHistoryList ();
		}


		private void UpdateTaskList()
		{
			m_tasks.Clear ();
			m_tasks.Add (m_unknown_task);
			var result = m_db.Table<TaskData> ();
			foreach (var i in result) {
				m_tasks.Add (i);
			}
		}

		private void UpdateTaskHistoryList()
		{
			m_histories.Clear ();
			m_raw_histories.Clear ();
//			var resulth = m_db.Table<TaskHistory> ();
			var today_day = new DateTime (DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
			var resulth = from x in m_db.Table<TaskHistory> () where x.End >= today_day || x.Begin >= today_day orderby x.Begin select x;
			CombinedTaskHistory current = null;
			foreach (var i in resulth) {
				if (current == null) {	// first
					// raw task
					m_raw_histories.Add (new CombinedTaskHistory () {
						Id = i.Id,
						TaskId = new ObservableCollection<int> () {
							i.TaskId
						},
						Begin = i.Begin,
						End = i.End,
						Rest = new TimeSpan (0)
					});

					// combind task
					current = new CombinedTaskHistory () {
						TaskId = new ObservableCollection<int> () {
							i.TaskId
						},
						Begin = i.Begin,
						End = i.End,
						Rest = new TimeSpan (0)
					};
					m_histories.Add (current);

				} else {
					/////////////////////////////////////////////////////////
					// raw task
					if (i.Begin - current.End >= new TimeSpan (0, 5, 0)) {	// more than 5 min rest
						m_raw_histories.Add (new CombinedTaskHistory () {
							Id = -1,	// not in DB
							TaskId = new ObservableCollection<int> () {
								-1
							},
							Begin = current.End,
							End = i.Begin,
							Rest = new TimeSpan (0)
						});
					}
					m_raw_histories.Add (new CombinedTaskHistory () {
						Id = i.Id,
						TaskId = new ObservableCollection<int>() {
							i.TaskId
						},
						Begin = i.Begin,
						End = i.End,
						Rest = new TimeSpan(0)
					});

					////////////////////////////////////////////////////////
					// combind task
					if ((from x in current.TaskId where x == i.TaskId select x).Count() != 0) {	// same task
						// combind task
						current.Rest += i.Begin - current.End;
						current.End = i.End;
					} else {	// different task
						if (i.End - i.Begin <= new TimeSpan (0, 30, 0) &&	// 30 minutes or less working time
							current.End - current.Begin <= new TimeSpan (1, 0, 0) && // 1hour or less combined tasks
							i.Begin - current.End <= new TimeSpan (0, 15, 0)) {	// 15min or less rest time

							// combine two different task when execution time is less
							current.TaskId.Add (i.TaskId);
							current.Rest += i.Begin - current.End;
							current.End = i.End;
						} else {	// new combined task
							if (i.Begin - current.End >= new TimeSpan (0, 5, 0)) {	// have 5 minutes span between two tasks

								// insert
								current = new CombinedTaskHistory () {
									TaskId = new ObservableCollection<int> () {
										-1
									},
									Begin = current.End,
									End = i.Begin,
									Rest = new TimeSpan (0)
								};
								m_histories.Add (current);
							}

							// Add New TaskHistory
							current = new CombinedTaskHistory () {
								TaskId = new ObservableCollection<int> (),
								Begin = i.Begin,
								End = i.End,
								Rest = new TimeSpan (0)
							};
							current.TaskId.Add (i.TaskId);
							m_histories.Add (current);
						}
					}
				}
			}
		}

		public void Go (TaskData task)
		{
			if (task != null) {
				m_data.current_task = task;
			}

			if (m_data.current_task == null)
				throw new NotImplementedException ();

			switch (m_data.state) {
			case 0:	// idle
			case 2: // pause
				m_data.current_exec = new TaskHistory ();
				m_data.current_exec.TaskId = m_data.current_task.Id;
				m_data.current_exec.Begin = DateTime.Now;
				m_data.current_exec.End = new DateTime ();
				Insert (m_data.current_exec);
				m_data.state = 1;	// working
				break;

			case 1:	// working
				m_data.current_exec.End = DateTime.Now;
				Update (m_data.current_exec);

				m_data.current_exec = new TaskHistory ();
				m_data.current_exec.TaskId = m_data.current_task.Id;
				m_data.current_exec.Begin = DateTime.Now;
				m_data.current_exec.End = new DateTime ();
				Insert (m_data.current_exec);
				break;

			default:
				break;
			}
		}

		private void StopExec()
		{
			m_data.current_exec.End = DateTime.Now;
			Update (m_data.current_exec);
			m_data.current_exec = null;
		}

		public void Pause ()
		{
			if (m_data.state == 1) {	// working
				StopExec ();
				m_data.state = 2;	// pause
			}
		}
		
		public void Finish ()
		{
			if (m_data.state == 1) {	// working
				StopExec ();
			}
			m_data.current_task = null;
			m_data.state = 0;	// idle
		}

		public void SaveState()
		{
			string folder = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			var path = System.IO.Path.Combine (folder, "model_data.json");

			try {
				var sw = new StreamWriter(path, false, Encoding.Unicode);
				if(sw != null) {
					sw.Write(Newtonsoft.Json.JsonConvert.SerializeObject (m_data));
					sw.Close();
				}
			} catch (Exception e) {
				Log.Debug ("RULY", e.Message);
			}
		}

		public void LoadState()
		{
			string folder = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			var path = System.IO.Path.Combine (folder, "model_data.json");

			try {
				var sw = new StreamReader(path, Encoding.Unicode);
				if(sw != null) {
					m_data = Newtonsoft.Json.JsonConvert.DeserializeObject<PersistentModelData>(sw.ReadToEnd());
					sw.Close();
				}
			} catch (Exception e) {
				Log.Debug ("RULY", e.Message);
			}
		}
	}
}

