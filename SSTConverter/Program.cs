using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SSTConverter
{
	public enum SentimentLabel
	{
		VeryNegative,
		Negative,
		Neutral,
		Positive,
		VeryPositive
	}

	public struct SentimentBucket
	{
		public SentimentLabel Label;
		public string DirectoryName;
		public string CachedPath;
		public int Counter;
	}

	class Program
	{
		static void Main(string[] args)
		{
			string inputPath = args[0];
			string outputPath = args[1];

			var outputDirectory = Directory.CreateDirectory(outputPath);

			foreach(FileInfo file in outputDirectory.GetFiles()) file.Delete();
			foreach(DirectoryInfo subDirectory in outputDirectory.GetDirectories()) subDirectory.Delete(true);

			string dictionaryPath = Path.Combine(inputPath, "dictionary.txt");
			string sentimentLabelsPath = Path.Combine(inputPath, "sentiment_labels.txt");

			var phrases = new Dictionary<int, string>();

			using(var reader = new StreamReader(dictionaryPath))
			{
				char[] separators = new[] { '|' };

				while(true)
				{
					string line = reader.ReadLine();
					if(line == null)
					{
						break;
					}

					string[] separated = line.Split(separators);
					string phraseText = separated[0];
					int phraseId = int.Parse(separated[1]);
					phrases.Add(phraseId, phraseText);
				}
				
			}

			var buckets = new List<SentimentBucket>()
			{
				new SentimentBucket { Label = SentimentLabel.VeryNegative, DirectoryName = "very_negative" },
				new SentimentBucket { Label = SentimentLabel.Negative, DirectoryName = "negative" },
				new SentimentBucket { Label = SentimentLabel.Neutral, DirectoryName = "neutral" },
				new SentimentBucket { Label = SentimentLabel.Positive, DirectoryName = "positive" },
				new SentimentBucket { Label = SentimentLabel.VeryPositive, DirectoryName = "very_positive" }
			};

			for(int i = 0; i < buckets.Count; ++i)
			{
				SentimentBucket bucket = buckets[i];

				bucket.CachedPath = Path.Combine(outputPath, bucket.DirectoryName);
				Directory.CreateDirectory(bucket.CachedPath);

				buckets[i] = bucket;
			}

			int lineCounter = 0;

			using(var reader = new StreamReader(sentimentLabelsPath))
			{
				char[] separators = new[] { '|' };

				while(true)
				{
					string line = reader.ReadLine();
					if(line == null)
					{
						break;
					}

					if(lineCounter == 0)
					{
						++lineCounter;
						continue;
					}
					else
					{
						++lineCounter;
					}

					string[] separated = line.Split(separators);
					int phraseId = int.Parse(separated[0]);
					float phraseSentiment = float.Parse(separated[1]);

					// sentiment_labels.txt contains all phrase ids and the corresponding sentiment labels, separated by a vertical line.
					// Note that you can recover the 5 classes by mapping the positivity probability using the following cut - offs:
					// [0, 0.2], (0.2, 0.4], (0.4, 0.6], (0.6, 0.8], (0.8, 1.0]
					// for very negative, negative, neutral, positive, very positive, respectively.

					SentimentLabel label;
					if(phraseSentiment <= 0.2f)
					{
						label = SentimentLabel.VeryNegative;
					}
					else if(phraseSentiment <= 0.4f)
					{
						label = SentimentLabel.Negative;
					}
					else if(phraseSentiment <= 0.6f)
					{
						label = SentimentLabel.Neutral;
					}
					else if(phraseSentiment <= 0.8f)
					{
						label = SentimentLabel.Positive;
					}
					else
					{
						label = SentimentLabel.VeryPositive;
					}

					for(int i = 0; i < buckets.Count; ++i)
					{
						SentimentBucket bucket = buckets[i];

						if(bucket.Label == label)
						{
							string phraseText = phrases[phraseId];
							int fileIndex = bucket.Counter;
							++bucket.Counter;
							buckets[i] = bucket;

							File.WriteAllText(Path.Combine(bucket.CachedPath, $"{fileIndex}.txt"), phraseText);

							break;
						}
					}
				}
			}
		}
	}
}
