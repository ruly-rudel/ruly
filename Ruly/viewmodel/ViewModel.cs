using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Ruly.model;
using System.Collections.ObjectModel;

namespace Ruly.viewmodel
{
	public class ViewModel {
		private static ViewModel me = new ViewModel();
		private Context m_ctx;
		private Model m_model;
		private int m_state;
		private TaskData m_selected_task;
		private CombinedTaskHistory m_selected_raw_task_history;
		private ObservableCollection<TaskAlarm> m_selected_task_alarm;

		private ViewModel()
		{
			m_ctx = Application.Context.ApplicationContext;
			m_model = new Model ();
			m_state = 0;
			m_selected_task_alarm = null;
		}

		public static ObservableCollection<TaskData> Tasks {
			get {
				return me.m_model.Tasks;
			}
		}

		public static ObservableCollection<CombinedTaskHistory> TaskHistories {
			get {
				return me.m_model.TaskHistories;
			}
		}

		public static ObservableCollection<CombinedTaskHistory> RawTaskHistories {
			get {
				return me.m_model.RawTaskHistories;
			}
		}

		public static IEnumerable<TaskAlarm> CurrentAlarms {
			get {
				return me.m_model.CurrentAlarms;
			}
		}

		public static TaskData SelectedTask {
			get {
				return me.m_selected_task;
			}
			private set {
				if (value == null || me.m_selected_task != value) {
					me.m_selected_task_alarm = null;
				}
				me.m_selected_task = value;
			}
		}
		
		public static CombinedTaskHistory SelectedRawTaskHistory {
			get {
				return me.m_selected_raw_task_history;
			}
			private set {
				me.m_selected_raw_task_history = value;
			}
		}
		
		public static ObservableCollection<TaskAlarm> SelectedTaskAlarm {
			get {
				if (me.m_selected_task_alarm == null) {
					if (me.m_selected_task == null) {
						return null;
					} else {
						return me.m_selected_task_alarm = new ObservableCollection<TaskAlarm> (me.m_model.GetAlarmFromTaskId(me.m_selected_task.Id));
					}
				} else {
					return me.m_selected_task_alarm;
				}
			}
		}

		public static DateTime ShowDate {
			get {
				return me.m_model.ShowDate;
			}
			set {
				me.m_model.ShowDate = value;
			}
		}

		public static TaskAlarm AddAlarm(int hour, int minute)
		{
			var alarm = new TaskAlarm() {
				Id = -1,
				TaskId = SelectedTask.Id,
				AlarmDateTime = new DateTime(1970, 1, 1, hour, minute, 0)
			};
			SelectedTaskAlarm.Add(alarm);
			me.m_model.Insert(alarm);
			return alarm;
		}

		public static void Update(TaskAlarm alarm)
		{
			me.m_model.Update (alarm);
		}

		// 0: idle, 1:go, 2:pause
		public static int State {
			get {
				return me.m_model.State;
			}
		} 

		public static TaskData GetTaskFromId (int taskId)
		{
			return me.m_model.GetTaskFromId (taskId);
		}

		public static TaskAlarm GetAlarmFromId (int id)
		{
			return me.m_model.GetAlarmFromId (id);
		}

		public static TaskData PrepareNewTask()
		{
			SelectedTask = new TaskData ();
			me.m_state = 0;	// idle
			return SelectedTask;
		}

		public static void SelectTask(int t)
		{
			SelectedTask = Tasks [t].Clone();
			me.m_state = 1; // selected;
		}

		public static void SelectRawTaskHistory (int t)
		{
			SelectedRawTaskHistory = RawTaskHistories [t].Clone ();
		}

		public static void UnselectTask()
		{
			SelectedTask = null;
			me.m_state = 0; // not selected
		}

		public static bool CommitTask()
		{
			switch (me.m_state) {
			case 0:	// idle
				me.m_model.Insert (SelectedTask);
				return true;

			case 1: // selected
				me.m_model.Update (SelectedTask);
				return true;

			default:
				throw new NotImplementedException ();
			}
		}

		public static void CommitRawTaskHistory ()
		{
			if (SelectedRawTaskHistory != null) {
				me.m_model.Update (SelectedRawTaskHistory);
				SelectedRawTaskHistory = null;
			}
		}

		public static void DiscardRawTaskHistory()
		{
			SelectedRawTaskHistory = null;
		}

		public static bool DiscardTask()
		{
			SelectedTask = null;
			switch (me.m_state) {
			case 0: // idle
				return true;

			case 1:	// selected
				throw new NotImplementedException ();

			default:
				throw new NotImplementedException ();
			}
		}

		public static void StartSelectedTask ()
		{
			me.m_model.Go (me.m_selected_task);
		}

		public static void Finish ()
		{
			me.m_model.Finish ();
		}

		public static void Pause ()
		{
			me.m_model.Pause ();
		}

		public static void Resume ()
		{
			me.m_model.Go (null);
		}

		public static void LoadState ()
		{
			me.m_model.LoadState ();
		}

		public static void SaveState ()
		{
			me.m_model.SaveState ();
		}
	}
}

