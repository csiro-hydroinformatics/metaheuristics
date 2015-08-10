#pragma once

#include <vector>
#include "core.h"


namespace mhcpp
{
	namespace objectives
	{
		static class Evaluations
		{
		private:
			Evaluations() {}
		public:
			~Evaluations() {}

			template<typename T>
			static std::vector<IObjectiveScores<T>> EvaluateScores(IObjectiveEvaluator<T>* evaluator, const std::vector<T>& population)
			{
				throw std::logic_error("Not implemented");
			}

		private:

		};
	}
}
