#pragma once

#include <vector>
#include "core.hpp"


namespace mhcpp
{
	namespace objectives
	{
		class Evaluations
		{
		private:
			Evaluations() {}
		public:
			~Evaluations() {}

			template<typename T>
			static std::vector<IObjectiveScores<T>> EvaluateScores(IObjectiveEvaluator<T>* evaluator, const std::vector<T>& population)
			{
				std::vector<IObjectiveScores<T>> result;
				for (size_t i = 0; i < population.size(); i++)
				{
					result.push_back(evaluator->EvaluateScore(population[i]));
				}
				return result;
				
			}

		};
	}
}
