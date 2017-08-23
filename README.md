# FaceRecognitionTracker

## ��������� ��������� �� Internet of Things

� ���� ���������� ��� ������������ ������� �������� �������, ������� ����� ����������� � ������ ���� ����������
� ���������� ������ � ������������� ���������.

### ��������� � ��������� ����������, ������������ ������������ ������

���������� ��������� � ���� ����������� GitHub. ��� ������ ���������� ��� �������, �������������� � ���������.

1. ���� �� ����������� ��������� ������ - ���������� ����������� � �����-������ ������� �����:

``` 
git clone https://github.com/evangelism/FaceRecognitionTracker
```

���� �� �� ����������� git, �� ������ ������� ZIP-������ ��������� ���� � ����� ���������� � � ����� ���������� �������.

2. �������� ���� FaceRecogitionTracker.sln � Visual Studio.
   �� ����� ������ ������������ ����� ���� "Open from source control..." � Visual Studio...
3. �������� ���� ��� ������������� Emotion API � Microsoft Cognitive Services. ������� �� 
   https://www.microsoft.com/cognitive-services/, �������� Emotions API -> Get Started for Free, � ��������
   ����.
4. � ����� Config.cs �������� ���� �� ���, ������� �� ������ ��� ��������.

��������� ������ - �� ������ �������, ��� ���� ��������� ������������, � � ���� "�����" ������������ ��������� � 
�������� � ������� JSON.

### ������������� IoT Hub

��� ������ ��������� � ������ �� ����� ������������ IoT Hub. ������� ������ ���������� Azure Portal http://portal.azure.com.

��� ������ ������� � ������ ���� IoT Hub (������ "�������� �����"). ����� �������� ���� ���������� ������ �����������:

![Get Access Key](images/IoTHub_AccessKeys.PNG)

### ������������ � IoT-���� � ������� ������ ����������� ��� ����������

����������� � ���������� [Device Explorer](https://github.com/Azure/azure-iot-sdks/blob/master/tools/DeviceExplorer/doc/how_to_use_device_explorer.md). � ��� �������
������ ����������� � IoT-����:

![Device Explorer 1](images/DeviceExplorer1.PNG)

����� ����� ��������� �� ������� "Management" � �������� ����� ����������. ����� ������ ������� ������� �� ������ � ����������� � �������� "Copy Connection String".

![Device Explorer 2](images/DeviceExplorer2.PNG)

## �������� ��� ��� ������ � IoT-�����

����� ������� ��������� [Getting Started](https://azure.microsoft.com/ru-ru/develop/iot/get-started/) ���� � MSDN. 
��������� ��� ����������, ���� ���������������� � �.�. - � ��������� �������� ����.

� ����� ������ �������� Raspberry Pi 2 -> Windows -> C#. (��� ��� Raspberry Pi �������� � ��� ����������� UWP-����������). 
��� ���������� ���� � ������, ���������� ����� �������� ������ �� NuGet-�����
`Microsoft.Azure.Devices.Client`. � ���������� ���� �� �������� ��������� ������ ����������� �� ��, 
������� ���� �������� �� ���������� ����.

### �������� ������ � IoT Hub

��� �������� ������ ������������ ��������� ���:

```
iothub = DeviceClient.CreateFromConnectionString(DeviceConnectionString);
await iothub.OpenAsync();
...
var b = Encoding.UTF8.GetBytes(s);
await iothub.SendEventAsync(new Message(b));
```
����� `s` - ��� ������, ���������� JSON-��� ��� ������.

### ������� �������
���� �� ������������� ����������������� ������� ������ � ������ �������������� - �����������
branch *MicrosoftBuzzwordDaySolution*, ���������� ������� ������� �� ������� ������ � IoT-hub
(�� �������� �������� ������ �����������!).

### ��������� ������ �� IoT Hub
��� ����� ��������� �� IoT Hub ����� ������������ ��������� ���:
```
        private async Task Receive()
        {
            while (true)
            {
                var msg = await iothub.ReceiveAsync();
                if (msg != null)
                {
                    var s = Encoding.ASCII.GetString(msg.GetBytes());
                    // ������� ���-�� � ���������� ����������, ��������, ������ ���������
                    await iothub.CompleteAsync(msg);
                }
            }
        }
```

## ����������� Stream Analytics

��� �������� ������ �� IoT Hub � ������� �������� ����� ������������ Stream Analytics.

��� ������, �������� Stream Analytics ��� �������� ������ �� IoT Hub � PowerBI. ������ ����� ����������� ���������� ������ �� ����������� �� �����-�� �������� �������,
��������, 5 ������.

��� ����� ���������� ������� ������ Stream Analytics, ���������������� � ��� ������� � �������� ������ ������, � ������ ������. 

� �������� ������� ������ ���������� IoT Hub, ������� ������� ������ `InHub`. 
� �������� �������� ������ - Power BI, ������ �� `OutBI`. 
�������� ��������, ��� � ������� ������ ������� Azure ��� ���������������� PowerBI ���������� ������������ ������ ������ http://manage.windowsazure.com.

� �������� ����������� ������� ����� �������� �������� ������, �������� ���������� �������� ��������� � ��������� ������:

```
SELECT * INTO [OutBI] FROM [InHub]
```

��� ���������� ������ �� 5 ������ ���������� ����� ������:

```
SELECT
    AVG(Happiness) as AVHappiness,
    AVG(Surprise) as AVSurpive,
    ...
    MAX(Time) as EndTime, MIN(Time) as BeginTime
INTO [OutBI]
FROM [InHub] TIMESTAMP BY Time
GROUP BY TumblingWindow(Duration(second,5))
```

��� ����� ���������� [���������� �������� � ������� �������� ��������](https://azure.microsoft.com/en-us/documentation/articles/stream-analytics-stream-analytics-query-patterns/).

����� ���������������� ������� Stream Analytics ���������� ��������� ������� �� ����������.

## ����������� ����� � PowerBI

����� ����, ��� Stream Analytics ����� ��������, �� ������ ������� � ������ PowerBI ��������� ������:

![PowerBI](images/PowerBI.PNG)
