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
	}
}