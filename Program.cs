using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pilot
{ 
	/// <summary>
	/// Банк для взаимодействия
	/// </summary>
	enum EBankType
	{
		Sberbank = 666,
	}

	/// <summary>
	/// Настройки терминала
	/// </summary>
	class PilotOption
	{
		private int comPort = 3;
		public int ComPort { get => comPort; set => comPort = value; }

		private string printerFile = "p";
		public string PrinterFile { get => printerFile; set => printerFile = value; }
		
		private string printerEnd = "01";
		public string PrinterEnd { get => printerEnd; set => printerEnd = value; }
		
		private bool pinpadLog = false;
		public bool PinpadLog { get => pinpadLog; set => pinpadLog = value; }
		
		private int department = 0;
		public int Department { get => department; set => department = value; }
		
		private int speed = 115200;
		public int Speed { get => speed; set => speed = value; }
		
		private bool showScreens = true;
		public bool ShowScreens { get => showScreens; set => showScreens = value; }
		
		private bool newProtocol = true;
		public bool NewProtocol { get => newProtocol; set => newProtocol = value; }

		private string fileName = @"pinpad.ini";
		/// <summary>
		/// Наименование конфиг файла
		/// </summary>
		public string FileName { get => fileName; set => fileName = value; }
		
		private string path = @"C:\Users\karas\Desktop\сбер";
		/// <summary>
		/// путь к драйверу
		/// </summary>
		public string Path { get => path; set => path = value; }

		public int Save()
		{
			try
			{
				using (StreamWriter sw = new StreamWriter((path + @"\" + fileName).Replace(@"\\\", @"\").Replace(@"\\", @"\"), false, System.Text.Encoding.Default))
				{
					sw.WriteLine(
						$@" " +
						"\n" + $@"ComPort={comPort} " +
						"\n" + $@"PrinterFile={printerFile}" +
						"\n" + $@"PrinterEnd={printerEnd}" +
						"\n" + $@";PinpadLog={(pinpadLog ? '1' : '0')}" +
						"\n" + $@";Department={department}" +
						"\n" + $@"Speed={speed}" +
						"\n" + $@"ShowScreens={(showScreens ? '1' : '0')}" +
						"\n" + $@"NewProtocol={(newProtocol ? '1' : '0')}" +

						"\n" + @";PrinterEnd=010D0A" +
						"\n" + @";PrinterEnd=1B37" +
						"\n" + @";PrinterEnd=0D56N" +
						"\n" + @";PrinterEnd=010D0A080D0A" +
						"\n" + @";PrinterEnd=1B550D0A" +
						"\n" + @";PrinterEnd=7E53" +
						"\n" + @";PrinterEnd=22" +
						"\n" + @";PrinterEnd=24" +
						"\n" + @";PrinterEnd=1B69" +
						"\n" + @";PrinterEnd=1B55" +
						"\n" + @";PinpadIPAddr=192.168.1.999" +
						"\n" + @";PinpadIPPort=8888" +
						"\n" + @";TerminalID=00000000" +
						"\n" + @";MerchantID=000000000000" +
						"\n" + @";CardHolderSignatureImage=sign\<date}\<t_id}\<tn}-<time}.png" +
						"\n" + @";PrinterType=Shtrih-PTRK1" +
						"\n" + @";PrinterType=Epson-TM950" +
						"\n" + @";PrinterType=Generic_32_chars" +
						"\n" + @";PrinterType=Generic_35_chars" +
						"\n" + @";PrinterType=Generic_36_chars" +
						"\n" + @";PrinterType=Generic_40_chars" +
						"\n" + @";ForceTopMost=1" +

						@""

					);
				}
				return 0;
			}
			catch (Exception)
			{
				return 1;
			}
			

			
		}
	}

	/// <summary>
	/// Тип результата выполнения операции
	/// </summary>
	enum EPilotOperationResultType
	{
		OK = 0,
		Error = 1,
	}
	/// <summary>
	/// Результат выполнения операции терминала
	/// </summary>
	class PilotOperationResult
	{
		static public PilotOperationResult Error = new PilotOperationResult { ResultCode = EPilotOperationResultType.Error, ResultString = "Error" };

		private EPilotOperationResultType resultCode = EPilotOperationResultType.OK;
		private string resultString = "";
		private string e_file = "";
		private string p_file = "";

		public EPilotOperationResultType ResultCode { get => resultCode; set => resultCode = value; }
		public string ResultString { get => resultString; set => resultString = value; }

		public string E_file { get => e_file; set => e_file = value; }
		public string P_file { get => p_file; set => p_file = value; }

	}

	/// <summary>
	/// ОбЪект для взаимодействия с терминалом
	/// </summary>
	class Pilot
	{
		#region переменные

		/// <summary>
		/// Выбранный банк, с которым будет работать терминал
		/// </summary>
		private EBankType bankType = EBankType.Sberbank;
		/// <summary>
		/// Параметры терминала
		/// </summary>
		private PilotOption pilotOption;
		/// <summary>
		/// Результат последней операции
		/// </summary>
		private PilotOperationResult lastOperationResult;

		#endregion

		#region свойства
		
		/// <summary>
		/// Выбранный банк, с которым будет работать терминал
		/// </summary>
		public EBankType BankType { get => bankType; set => bankType = value; }
		/// <summary>
		/// Параметры терминала
		/// </summary>
		public PilotOption PilotOption { get => pilotOption; set => pilotOption = value; }
		/// <summary>
		/// Результат последней операции
		/// </summary>
		public PilotOperationResult LastOperationResult { get => lastOperationResult; set => lastOperationResult = value; }

		#endregion

		/// <summary>
		/// Чтение текста из файла с определением кодировки файла по ключевому слову
		/// </summary>
		/// <param name="path">Путь к файлу</param>
		/// <param name="validationKey">Ключевое слово</param>
		/// <param name="encodingKeys">Список CodePage кодировок по которым необходимо искать</param>
		/// <returns>Строка приведенная к корректному читаемому виду</returns>
		private string ReadFileValidString(string path, string validationKey, int[] encodingKeys = null)
		{
			try
			{
				foreach (EncodingInfo ei in Encoding.GetEncodings())
					if (encodingKeys == null || encodingKeys.Contains(ei.GetEncoding().CodePage))
						using (StreamReader sr = new StreamReader(path, ei.GetEncoding(), false))
						{
							var result = sr.ReadToEnd();
							if (result.ToUpper().Contains(validationKey.ToUpper()))
								return result;
						}

				using (StreamReader sr = new StreamReader(path, Encoding.Default, false)) { return sr.ReadToEnd(); }
			}
			catch
			{
				return string.Empty;
			}
		}
		/// <summary>
		/// Запуск операции терминала оплаты
		/// </summary>
		/// <param name="pilotOption">Параметры терминала</param>
		/// <param name="cmd">Строка с командлетом</param>
		/// <param name="param">Строка с набором параметров</param>
		/// <returns>Результат выполнения операции</returns>
		private PilotOperationResult Run(PilotOption pilotOption, string cmd, string param) 
		{
			#region Подготовка

			pilotOption.Save(); // Запись переданной конфигурации в конфигурационный файл

			string path = pilotOption.Path;

			// Составление команды для терминала
			cmd = (path + cmd).Replace(@"\\\", @"\").Replace(@"\\", @"\");

			// Отправка команды в терминал
			System.Diagnostics.Process.Start(cmd, param);

			// Обявление обекта для отслеживания выполнения отправленной на терминал операции
			bool _isWait = true;

			#endregion

			#region Ожидание выполнения операции

			// отслеживаем изменение файла с результатом выполнения
			using (FileSystemWatcher watcher = new FileSystemWatcher())
			{
				watcher.Path = path;

				watcher.NotifyFilter = NotifyFilters.LastAccess
									 | NotifyFilters.LastWrite
									 | NotifyFilters.FileName
									 | NotifyFilters.DirectoryName;

				watcher.Filter = "e";
				watcher.Changed += (sourse, e) => { _isWait = false; };
				watcher.EnableRaisingEvents = true;

				// ждем записи в файл с результатами
				while (_isWait) ;

			}

			#endregion

			#region Выдача результата выполнения

			string e_file = "";
			string p_file = "";

			int resultCode; // код-результат выполнения операции
			int resultFinanceOperationsCount; // количество проведенных финансовых операций


			#region работа с e-файлом

			// чтение файла e - с результатом выполнения последней опирации
			try
            {
				using (var sr = new StreamReader(path + @"\e")) { e_file = sr.ReadToEnd(); }
			}
			catch (Exception e)
			{
				return lastOperationResult = new PilotOperationResult
				{
					ResultCode = EPilotOperationResultType.Error,
					ResultString = e.Message,
					E_file = e_file,
					P_file = p_file
				};
			}

			// Определение кода выполнения операции
			try { resultCode = int.Parse(e_file.Split("\n".ToCharArray(), StringSplitOptions.None)[0]); }
			catch (Exception) { resultCode = 2000; }

			// Определение количества проведенных финансовых операций
			try { resultFinanceOperationsCount = int.Parse(e_file.Split("\n".ToCharArray(), StringSplitOptions.None)[1]); }
			catch (Exception) { resultFinanceOperationsCount = 0; }

			#endregion



			#region работа с p-файлом

			// чтение файла p - с чеком последней опирации
			try
			{
				p_file = ReadFileValidString(
					path: path + @"\" + pilotOption.PrinterFile,
					validationKey: "Терминал",
					encodingKeys: new int[] { 866, 1251 });
			}
			catch (Exception e)
			{
				return lastOperationResult = new PilotOperationResult
				{
					ResultCode = EPilotOperationResultType.Error,
					ResultString = e.Message,
					E_file = e_file,
					P_file = p_file
				};
			}

			#endregion

			return lastOperationResult = new PilotOperationResult
			{
				ResultCode = 
					resultCode == 0 ? 
						EPilotOperationResultType.OK : 
						EPilotOperationResultType.Error,
				
				ResultString =
					resultCode == 0 ?
						"Успешное выполнение операции." : 
						"Операция не выполнена.",

				E_file = e_file,

				P_file = p_file
			};

			#endregion

			#region примеры чеков по операциям

			/*
				-- оплата
				0
				5
				0


						 ТЕСТОВЫЙ POS ТЕРМИНАЛ
								 Город
								 Улица
						   т.8(800)350-01-23
				11.09.20     11:05            ЧЕК   0005
				ПАО СБЕРБАНК                      Оплата
				Терминал: 00459991 Мерчант: 454444445555
						   Visa Credit    A0000000031010
				Карта:(E1)              ************8943
				Клиент:                                /
				Сумма (Руб):                      100.84
				Комиссия за операцию - 0 Руб.
								ОДОБРЕНО
				Код авторизации:                  10P425
				Номер ссылки:               159981153122
					  Подпись клиента не требуется
				113590E050AA564881B68D00ACAFBDC0161623D6
				========================================

				-- отмена
				0
				5
				0


						 ТЕСТОВЫЙ POS ТЕРМИНАЛ
								 Город
								 Улица
						   т.8(800)350-01-23
				11.09.20     11:05            ЧЕК   0005
				ПАО СБЕРБАНК                      Отмена
				Терминал: 00459991 Мерчант: 454444445555
						   Visa Credit    A0000000031010
				Карта:(E1)              ************8943
				Клиент:                                /
				Сумма (Руб):                      100.84
				Комиссия за операцию - 0 Руб.
								ОДОБРЕНО
				Код авторизации:                  10P425
				Номер ссылки:               159981153122
				========================================

				-- возврат
				0
				6
				0


						 ТЕСТОВЫЙ POS ТЕРМИНАЛ
								 Город
								 Улица
						   т.8(800)350-01-23
				11.09.20     11:07            ЧЕК   0006
				ПАО СБЕРБАНК                     Возврат
				Терминал: 00459991 Мерчант: 454444445555
						   Visa Credit    A0000000031010
				Карта:(E)               ************8943
				Клиент:                                /
				Сумма (Руб):                      100.84
				Комиссия за операцию - 0 Руб.
				Номер ссылки:               111111111111


				   ________________________________
							подпись клиента
				5DC51BB11DD3BA80F86EBDE97C140EA2A5A28148
				Срок возврата до 5 дней.
				Проверьте зачисление в Отчете по карте
				========================================

				-- контрольная лента 
				0


						 ТЕСТОВЫЙ POS ТЕРМИНАЛ
								 Город
								 Улица
						   т.8(800)350-01-23
				11.09.20                           11:08
				ПАО СБЕРБАНК           Контрольная лента
				Терминал: 00459991 Мерчант: 454444445555
				----------------------------------------

				Валюта    :                          Руб

				 Оплата

				   11.09.20 10:12          0001
				   Kарта:     ************8943 (E1)
				   Тип карты:       Visa Credit
				   Код авторизации:      49L215
				   Сумма:                100.00

				 Всего операций:              1
				   на сумму:             100.00
				   Скидка:                 0.00

				 Возврат

				   11.09.20 10:45          0004
				   Kарта:     ************8943 (E)
				   Тип карты:       Visa Credit
				   Код авторизации:      57A978
				   Сумма:                100.84

				   11.09.20 11:07          0006
				   Kарта:     ************8943 (E)
				   Тип карты:       Visa Credit
				   Код авторизации:      69Z817
				   Сумма:                100.84

				 Всего операций:              2
				   на сумму:             201.68

				 Отмена

				 Всего операций:              3
				   на сумму:             302.52
				----------------------------------------
				***********  Отчет закончен  ***********
				****************************************
						  База знаний кассира
						   Оплата-картой.рф
				****************************************

				========================================

				-- сверка итогов
				0


						 ТЕСТОВЫЙ POS ТЕРМИНАЛ
								 Город
								 Улица
						   т.8(800)350-01-23
				11.09.20                           11:09
				ПАО СБЕРБАНК               Сверка итогов
				Терминал: 00459991 Мерчант: 454444445555
				----------------------------------------
				Итоги совпали
				----------------------------------------

				Валюта    :                          Руб

				 Оплата

				 Всего операций:              1
				   на сумму:             100.00
				   Скидка:                 0.00

				 Возврат

				 Всего операций:              2
				   на сумму:             201.68

				 Отмена

				 Всего операций:              3
				   на сумму:             302.52
				----------------------------------------
				***********  Отчет закончен  ***********
				****************************************
						  База знаний кассира
						   Оплата-картой.рф
				****************************************

				======================================== 

			 * 			 
			 */

			#endregion
		}

        /// <summary>
        /// Операция подведения итогов для банка "Сбербанк"
        /// </summary>
        /// <returns>PilotOperationResult</returns>
        private PilotOperationResult CheckResultsSberbank()
		{
			return Run(
				pilotOption: pilotOption,
				cmd: @"\sbcall", 
				param: @"6000");
		}
		/// <summary>
		/// Операция сверки итогов
		/// </summary> 
		/// <returns>Результат выполнения операции</returns>
		public PilotOperationResult CheckResults()
		{
			switch (bankType)
			{
				case EBankType.Sberbank: return CheckResultsSberbank();
				default: return PilotOperationResult.Error;
			}
		}

		/// <summary>
		/// Операция получения контрольной ленты для банка "Сбербанк"
		/// </summary>
		/// <returns>PilotOperationResult</returns>
		private PilotOperationResult ControlTapeSberbank()
		{
			return Run(
				pilotOption: pilotOption,
				cmd: @"\sbcall",
				param: @"7000");
		}
		/// <summary>
		/// Операция контрольной ленты
		/// </summary> 
		/// <returns>Результат выполнения операции</returns>
		public PilotOperationResult ControlTape()
		{
			switch (bankType)
			{
				case EBankType.Sberbank: return ControlTapeSberbank();
				default: return PilotOperationResult.Error;
			}
		}

		/// <summary>
		/// Операция возврата для банка "Сбербанк"
		/// </summary>
		/// <returns>PilotOperationResult</returns>
		private PilotOperationResult RefundSberbank(decimal price)
		{
			return Run(
				pilotOption: pilotOption,
				cmd: @"\sbcall",
				param: $@"4002 {(int)(price * 100)} 1 1");
		}
		/// <summary>
		/// Операция возврата
		/// </summary>
		/// <param name="price">сумма возврата</param>
		/// <returns>Результат выполнения операции</returns>
		public PilotOperationResult Refund(decimal price)
		{
			switch (bankType)
			{
				case EBankType.Sberbank: return RefundSberbank(price);
				default: return PilotOperationResult.Error;
			}
		}
		/// <summary>
		/// Операция возврата
		/// </summary>
		/// <param name="price">сумма возврата</param>
		/// <returns>Результат выполнения операции</returns>
		public PilotOperationResult Refund(double price) => Refund((decimal)price);

		/// <summary>
		/// Операция покупка для банка "Сбербанк"
		/// </summary>
		/// <returns>PilotOperationResult</returns>
		private PilotOperationResult PaySberbank(decimal price)
		{
			return Run(
				pilotOption: pilotOption,
				cmd: @"\sbcall",
				param: $@"4000 {(int)(price * 100)} 1 1");
		}
		/// <summary>
		/// Операция оплаты
		/// </summary>
		/// <param name="price">сумма оплаты</param>
		/// <returns>Результат выполнения операции</returns>
		public PilotOperationResult Pay(decimal price)
		{
			switch (bankType)
			{
				case EBankType.Sberbank: return PaySberbank(price);
				default: return PilotOperationResult.Error;
			}
		}
		/// <summary>
		/// Операция оплаты
		/// </summary>
		/// <param name="price">сумма оплаты</param>
		/// <returns>Результат выполнения операции</returns>
		public PilotOperationResult Pay(double price) => Pay((decimal)price);

		/// <summary>
		/// Операция отмена для банка "Сбербанк"
		/// </summary>
		/// <returns>PilotOperationResult</returns>
		private PilotOperationResult CancelSberbank(decimal price)
		{
			return Run(
				pilotOption: pilotOption,
				cmd: @"\sbcall",
				param: $@"4003 {(int)(price * 100)} 1 1");
		}
		/// <summary>
		/// Операция отмены
		/// </summary>
		/// <param name="price">сумма отмены</param>
		/// <returns>Результат выполнения операции</returns>
		public PilotOperationResult Cancel(decimal price)
		{
			switch (bankType)
			{
				case EBankType.Sberbank: return CancelSberbank(price);
				default: return PilotOperationResult.Error;
			}
		}
		/// <summary>
		/// Операция отмены
		/// </summary>
		/// <param name="price">сумма отмены</param>
		/// <returns>Результат выполнения операции</returns>
		public PilotOperationResult Cancel(double price) => Cancel((decimal)price);
	}
}

namespace Pilot
{
	class Program
	{
		static void Main(string[] args)
		{
			var pilotOption = new PilotOption
			{
				ComPort = 3,
				PrinterFile = "p",
				PinpadLog = true,
				Speed = 115200,
				ShowScreens = true,
				NewProtocol = true,

				Path = @"C:\Users\karas\Desktop\сбер",
			};

			Pilot pilot = new Pilot
			{
				BankType = EBankType.Sberbank,
				PilotOption = pilotOption
			};

			ConsoleKey key;

			do {
				Console.WriteLine("Для выхода нажмите: ------------------ Escape");
				Console.WriteLine("Для сверки итогов нажмите: ----------- 1");
				Console.WriteLine("Для вызова контроьной ленты нажмите: - 2");
				Console.WriteLine("Для возврата нажмите: ---------------- 3");
				Console.WriteLine("Для оплаты нажмите: ------------------ 4");
				Console.WriteLine("Для отмены нажмите: ------------------ 5");

				key = Console.ReadKey().Key;
				Console.WriteLine("\n\n\n\n\n\n");


				PilotOperationResult result = null;

				if (key == ConsoleKey.D1)
				{
					result = pilot.CheckResults();
				}

				if (key == ConsoleKey.D2)
				{
					result = pilot.ControlTape();
				}

				if (key == ConsoleKey.D3)
				{
					result = pilot.Refund(100.84);
				}

				if (key == ConsoleKey.D4)
				{
					result = pilot.Pay(100.84);
				}

				if (key == ConsoleKey.D5)
				{
					result = pilot.Cancel(100.84);
				}

				if (result != null)
				{ 
					Console.WriteLine("ResultCode:   " + result.ResultCode.ToString());
					Console.WriteLine("ResultString: " + result.ResultString);

					Console.WriteLine(result.E_file);
					Console.WriteLine("");
					Console.WriteLine(result.P_file);
				} 

			} while (key != ConsoleKey.Escape);
		}
	}
}
