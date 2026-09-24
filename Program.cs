using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GeneticSearch
{
    struct GeneticData
    {
        public string protein;
        public string organism;
        public string amino_acids;
    }

    class Program
    {
        // ============ RLE ============

        static string RLEncoding(string amino_acids)
        {
            StringBuilder encoded = new StringBuilder();
            int i = 0;
            while (i < amino_acids.Length)
            {
                char ch = amino_acids[i];
                int count = 1;
                while (i + count < amino_acids.Length && amino_acids[i + count] == ch)
                    count++;

                if (count > 2)
                    encoded.Append(count).Append(ch);
                else
                {
                    for (int j = 0; j < count; j++)
                        encoded.Append(ch);
                }
                i += count;
            }
            return encoded.ToString();
        }

        static string RLDecoding(string amino_acids)
        {
            StringBuilder decoded = new StringBuilder();
            int i = 0;
            while (i < amino_acids.Length)
            {
                char ch = amino_acids[i];
                if (char.IsDigit(ch))
                {
                    int count = ch - '0';
                    char letter = amino_acids[i + 1];
                    decoded.Append(letter, count);
                    i += 2;
                }
                else
                {
                    decoded.Append(ch);
                    i++;
                }
            }
            return decoded.ToString();
        }

        // ============ Чтение данных ============

        static List<GeneticData> ReadData(string filename)
        {
            List<GeneticData> data = new List<GeneticData>();
            using (StreamReader reader = new StreamReader(filename))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] parts = line.Split('\t');
                    if (parts.Length < 3) continue;

                    GeneticData protein;
                    protein.protein = parts[0];
                    protein.organism = parts[1];
                    protein.amino_acids = RLDecoding(parts[2]);
                    data.Add(protein);
                }
            }
            return data;
        }

        // ============ Операции ============

        static void Search(List<GeneticData> data, string pattern,
                           StringBuilder output, int opNumber)
        {
            string cleanPattern = RLDecoding(pattern);
            output.AppendLine($"{opNumber:D3}   search   {cleanPattern}");
            output.AppendLine("organism                 protein");

            bool found = false;
            foreach (var protein in data)
            {
                if (protein.amino_acids.Contains(cleanPattern))
                {
                    output.AppendLine($"{protein.organism}\t{protein.protein}");
                    found = true;
                }
            }
            if (!found) output.AppendLine("NOT FOUND");
            output.AppendLine(new string('-', 74));
        }

        static void Diff(List<GeneticData> data, string name1, string name2,
                         StringBuilder output, int opNumber)
        {
            output.AppendLine($"{opNumber:D3}   diff   {name1}   {name2}");

            GeneticData? p1 = null, p2 = null;
            foreach (var protein in data)
            {
                if (protein.protein == name1) p1 = protein;
                if (protein.protein == name2) p2 = protein;
            }

            if (p1 == null || p2 == null)
            {
                output.Append("MISSING: ");
                if (p1 == null) output.Append(name1 + " ");
                if (p2 == null) output.Append(name2);
                output.AppendLine();
                output.AppendLine(new string('-', 74));
                return;
            }

            string a = p1.Value.amino_acids;
            string b = p2.Value.amino_acids;
            int diff = Math.Abs(a.Length - b.Length);
            for (int i = 0; i < Math.Min(a.Length, b.Length); i++)
                if (a[i] != b[i]) diff++;

            output.AppendLine("amino-acids difference:");
            output.AppendLine(diff.ToString());
            output.AppendLine(new string('-', 74));
        }

        static void Mode(List<GeneticData> data, string name,
                         StringBuilder output, int opNumber)
        {
            output.AppendLine($"{opNumber:D3}   mode   {name}");

            GeneticData? found = null;
            foreach (var protein in data)
            {
                if (protein.protein == name) { found = protein; break; }
            }

            if (found == null)
            {
                output.AppendLine("MISSING: " + name);
                output.AppendLine(new string('-', 74));
                return;
            }

            int[] counts = new int[26];
            foreach (char c in found.Value.amino_acids)
                if (c >= 'A' && c <= 'Z') counts[c - 'A']++;

            int maxCount = 0;
            char maxChar = 'A';
            for (int i = 0; i < 26; i++)
            {
                if (counts[i] > maxCount)
                {
                    maxCount = counts[i];
                    maxChar = (char)('A' + i);
                }
            }

            output.AppendLine("amino-acid occurs:");
            output.AppendLine($"{maxChar}\t{maxCount}");
            output.AppendLine(new string('-', 74));
        }

        // ============ Main ============

        static void Main(string[] args)
        {
            string sequencesFile = args.Length >= 1 ? args[0] : "sequences.1.txt";
            string commandsFile = args.Length >= 2 ? args[1] : "commands.1.txt";
            string outputFile = args.Length >= 3 ? args[2] : "genedata.txt";

            List<GeneticData> data = ReadData(sequencesFile);

            StringBuilder output = new StringBuilder();
            output.AppendLine("вась вась");
            output.AppendLine("Генетический поиск");
            output.AppendLine(new string('-', 74));

            int opNumber = 1;
            using (StreamReader reader = new StreamReader(commandsFile))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] parts = line.Split('\t');
                    switch (parts[0])
                    {
                        case "search": Search(data, parts[1], output, opNumber); break;
                        case "diff": Diff(data, parts[1], parts[2], output, opNumber); break;
                        case "mode": Mode(data, parts[1], output, opNumber); break;
                    }
                    opNumber++;
                }
            }

            File.WriteAllText(outputFile, output.ToString());
            Console.WriteLine($"Готово! Результат в {outputFile}");
        }
    }
}