using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.IO;
using UnityEngine;
/// <summary>
/// CSVの列とのマッピングのための属性クラス
/// </summary>
public class CsvColumnAttribute : Attribute
{
    public CsvColumnAttribute(int columnIndex)
        : this(columnIndex, null)
    {
    }
    public CsvColumnAttribute(int columnIndex, object defaultValue)
    {
        this.ColumnIndex = columnIndex;
        this.DefaultValue = defaultValue;
    }
    public int ColumnIndex { get; set; }
    public object DefaultValue { get; set; }
}

public class CSVReader<T> : IEnumerable<T>, IDisposable
    where T : class, new()
{
    public event EventHandler<ConvertFailedEventArgs> ConvertFailed;

    /// <summary>
    /// Type毎のデータコンバーター
    /// </summary>
    private Dictionary<Type, TypeConverter> converters = new Dictionary<Type, TypeConverter>();

    /// <summary>
    /// 列番号をキーとしてフィールド or プロパティへのsetメソッドが格納されます。
    /// </summary>
    private Dictionary<int, Action<object, string>> setters = new Dictionary<int, Action<object, string>>();

    /// <summary>
    /// Tの情報をロードします。
    /// setterには列番号をキーとしたsetメソッドが格納されます。
    /// </summary>
    private void LoadType()
    {
        Type type = typeof(T);

        // Field, Property のみを対象とする
        var memberTypes = new MemberTypes[] { MemberTypes.Field, MemberTypes.Property };

        // インスタンスメンバーを対象とする
        BindingFlags flag = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        foreach (MemberInfo member in type.GetMembers(flag).Where((member) => memberTypes.Contains(member.MemberType)))
        {
            CsvColumnAttribute csvColumn = GetCsvColumnAttribute(member);

            if (csvColumn == null) continue;

            int columnIndex = csvColumn.ColumnIndex;
            object defaultValue = csvColumn.DefaultValue;



            if (member.MemberType == MemberTypes.Field)
            {
                // field
                FieldInfo fieldInfo = type.GetField(member.Name, flag);
                setters[columnIndex] = (target, value) =>
                    fieldInfo.SetValue(target, GetConvertedValue(fieldInfo, value, defaultValue));
            }
            else
            {
                // property
                PropertyInfo propertyInfo = type.GetProperty(member.Name, flag);
                setters[columnIndex] = (target, value) =>
                    propertyInfo.SetValue(target, GetConvertedValue(propertyInfo, value, defaultValue), null);
            }
        }
    }

    /// <summary>
    /// 対象のMemberInfoからCsvColumnAttributeを取得する
    /// </summary>
    /// <param name="member">確認対象のMemberInfo</param>
    /// <returns>CsvColumnAttributeのインスタンス、設定されていなければnull</returns>
    private CsvColumnAttribute GetCsvColumnAttribute(MemberInfo member)
    {
        return (CsvColumnAttribute)member.GetCustomAttributes(typeof(CsvColumnAttribute), true).FirstOrDefault();
        //return member.GetCustomAttributes<CsvColumnAttribute>().FirstOrDefault();
    }

    /// <summary>
    /// valueを対象のTypeへ変換する。できない場合はdefaultを返す
    /// </summary>
    /// <param name="type">変換後の型</param>
    /// <param name="value">変換元の値</param>
    /// <param name="default">規定値</param>
    /// <returns></returns>
    private object GetConvertedValue(MemberInfo info, object value, object @default)
    {
        Type type = null;
        if (info is FieldInfo)
        {
            type = (info as FieldInfo).FieldType;
        }
        else if (info is PropertyInfo)
        {
            type = (info as PropertyInfo).PropertyType;
        }

        // コンバーターは同じTypeを使用することがあるため、キャッシュしておく
        if (!converters.ContainsKey(type))
        {
            converters[type] = TypeDescriptor.GetConverter(type);
        }

        TypeConverter converter = converters[type];

        ////変換できない場合に例外を受け取りたい場合
        //return converter.ConvertFrom(value);

        //失敗した場合に CsvColumnAttribute の規定値プロパティを返す場合
        try
        {
            // 変換した値を返す。
            return converter.ConvertFrom(value);
        }
        catch (Exception)
        {
            // 変換できなかった場合は規定値を返す
            return @default;
        }

        //// 変換できない場合に、イベントを発生させ使用者に判断させる場合
        //try
        //{
        //    return converter.ConvertFrom(value);
        //}
        //catch (Exception ex)
        //{
        //    // イベント引数の作成
        //    var e = new ConvertFailedEventArgs(info, value, @default, ex);

        //    // イベントに関連付けられたメソッドがない場合は例外を投げる
        //    if (ConvertFailed == null)
        //    {
        //        throw;
        //    }

        //    // 使用する際に判断させる
        //    ConvertFailed(this, e);

        //    // 正しい値を返す
        //    return e.CorrectValue;
        //}
    }


    private StringReader reader;
    private string filePath;
    private bool skipFirstLine;
    private Encoding encoding;

    public CSVReader(string filePath)
        : this(filePath, true)
    {
    }

    public CSVReader(string filePath, bool skipFirstLine)
        : this(filePath, skipFirstLine, null)
    {
    }

    public CSVReader(string filePath, bool skipFirstLine, Encoding encoding)
    {
        /*
        // 拡張子の確認
        if (!filePath.EndsWith(".csv", StringComparison.CurrentCultureIgnoreCase))
        {
            throw new FormatException("拡張子が.csvでないファイル名が指定されました。");
        }*/

        this.filePath = filePath;
        this.skipFirstLine = skipFirstLine;
        this.encoding = encoding;

        // 既定のエンコードの設定
        if (this.encoding == null)
        {
            this.encoding = System.Text.Encoding.GetEncoding("utf-8");
        }

        // Tを解析する
        LoadType();
        TextAsset csv = Resources.Load(this.filePath) as TextAsset;
        //this.reader = new StreamReader(this.filePath, this.encoding);

        this.reader = new StringReader(csv.text);
        // ヘッダーを飛ばす場合は1行読む
        if (skipFirstLine)
        {
            this.reader.ReadLine();
        }
    }

    public void Dispose()
    {
        using (reader)
        {
        }
        reader = null;
    }

    public IEnumerator<T> GetEnumerator()
    {
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            // T のインスタンスを作成
            var data = new T();

            // 行をセパレータで分解
            string[] fields = line.Split(',');

            // セル数分だけループを回す
            foreach (int columnIndex in Enumerable.Range(0, fields.Length))
            {
                // 列番号に対応するsetメソッドがない場合は処理しない
                if (!setters.ContainsKey(columnIndex)) continue;

                // setメソッドでdataに値を入れる
                setters[columnIndex](data, fields[columnIndex]);
            }

            yield return data;
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}

/// <summary>
/// 変換失敗時のイベント引数クラス
/// </summary>
public class ConvertFailedEventArgs : EventArgs
{
    public ConvertFailedEventArgs(MemberInfo info, object value, object defaultValue, Exception ex)
    {
        this.MemberInfo = info;
        this.FailedValue = value;
        this.CorrectValue = defaultValue;
        this.Exception = ex;
    }

    /// <summary>
    /// 変換に失敗したメンバーの情報
    /// </summary>
    public MemberInfo MemberInfo { get; private set; }

    /// <summary>
    /// 失敗時の値
    /// </summary>
    public object FailedValue { get; private set; }

    /// <summary>
    /// 正しい値をイベントで受け取る側が設定してください。規定値はCsvColumnAttribute.DefaultValueです。
    /// </summary>
    public object CorrectValue { get; set; }

    /// <summary>
    /// 発生した例外
    /// </summary>
    public Exception Exception { get; private set; }
}



