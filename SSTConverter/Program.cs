using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SSTConverter
{
	public enum SentimentLabel
	{
		Negative,
		Neutral,
		Positive
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

			string negativePath = Path.Combine(outputPath, "negative");
			string neutralPath = Path.Combine(outputPath, "neutral");
			string positivePath = Path.Combine(outputPath, "positive");

			Directory.CreateDirectory(negativePath);
			Directory.CreateDirectory(neutralPath);
			Directory.CreateDirectory(positivePath);

			int negativePhraseCounter = 0;
			int neutralPhraseCounter = 0;
			int positivePhraseCounter = 0;

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
					if(phraseSentiment <= 0.4f)
					{
						label = SentimentLabel.Negative;
					}
					else if(phraseSentiment <= 0.6f)
					{
						label = SentimentLabel.Neutral;
					}
					else
					{
						label = SentimentLabel.Positive;
					}

					string sentimentPath = null;
					int fileIndex = 0;

					switch(label)
					{
						case SentimentLabel.Negative:
							sentimentPath = negativePath;
							fileIndex = negativePhraseCounter++;
							break;
						case SentimentLabel.Neutral:
							sentimentPath = neutralPath;
							fileIndex = neutralPhraseCounter++;
							break;
						case SentimentLabel.Positive:
							sentimentPath = positivePath;
							fileIndex = positivePhraseCounter++;
							break;
					}

					string phraseText = phrases[phraseId];
					File.WriteAllText(Path.Combine(sentimentPath, $"{fileIndex}.txt"), phraseText);
				}
			}
		}
	}
}
