#pragma once

#include <string>
#include <set>
#include <vector>
#include <map>
#include <algorithm>
#include <iostream>
#include <iterator>

namespace mhcpp
{
	namespace utils
	{
		template<typename T>
		bool AreSetEqual(const std::vector<T>& a, const std::vector<T>& b)
		{
			std::set<T> sa(a.begin(), a.end());
			std::set<T> sb(b.begin(), b.end());
			return(sa == sb);
		}

		template<typename K, typename V>
		const K& GetKey(const std::pair<K, V>& keyValue)
		{
			return keyValue.first;
		}

		template<typename K, typename V>
		const std::vector<K> GetKeys(const std::map<K, V>& aMap)
		{
			std::vector<K> keys(aMap.size());
			transform(aMap.begin(), aMap.end(), keys.begin(), GetKey<K, V>);
			return keys;
		}

		template<typename ElemType>
		void PrintHistogram(const std::vector<ElemType>& hist, std::ostream& stream, int nstars = 100, char c = '*')
		{
			ElemType total = std::accumulate(hist.begin(), hist.end(), 0);
			size_t n = hist.size();
			std::vector<string> s(n);
			for (size_t i = 0; i < n; ++i)
				s[i] = std::string(hist[i] * nstars / total, c);
			PrintVec(s);
		}

		template<typename ElemType>
		std::vector<double> Normalize(const std::vector<ElemType>& hist)
		{
			size_t n = hist.size();
			std::vector<double> p(n);
			ElemType total = std::accumulate(hist.begin(), hist.end(), 0);
			for (size_t i = 0; i < n; ++i)
				p[i] = (double)hist[i] / total;
			return p;
		}

		template<typename ElemType>
		std::vector<ElemType> RelativeDiff(const std::vector<ElemType>& expected, const std::vector<ElemType>& b)
		{
			std::vector<ElemType> result(expected.size());
			for (size_t i = 0; i < expected.size(); i++)
			{
				result[i] = (std::abs(expected[i] - b[i]) / expected[i]);
			}
			return result;
		}

		template<typename ElemType>
		void PrintVec(const std::vector<ElemType>& hist, std::ostream& stream)
		{
			int n = hist.size();
			for (size_t i = 0; i < n; ++i)
				stream << i << ": " << std::to_string(hist[i]) << std::endl;
		}

		template<typename ElemType>
		void PrintValues(const std::vector<ElemType>& hist, std::ostream& stream, bool proportions = false)
		{
			int n = hist.size();
			if (!proportions)
			{
				PrintVec(hist, stream);
			}
			else
			{
				auto p = Normalize(hist);
				PrintVec(p, stream);
			}
		}
	}
}