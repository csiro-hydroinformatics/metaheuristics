context("system configuration i.e. parameterisation")

test_that("bounds and values", {
  testHc <- clrNew('CSIRO.Metaheuristics.Tests.TestHyperCube', 3L, 1.0, 0.0, 2.0)
  setMinValue(testHc, as.character(1:2), 0.0 + 0.1*(1:2))
  setValue(testHc, as.character(0:2), 1.0 + 0.1*(0:2))

  expect_error(setMaxValue(testHc, as.character(0:1), 2.0))
  setMaxValue(testHc, as.character(0:1), 2.0 + 0.1*(0:1))

  expect_equal(0+0.1*0:2      , getMinValue(testHc, as.character(0:2)))
  expect_equal(2+c(0.1*0:1, 0), getMaxValue(testHc, as.character(0:2)))
  expect_equal(1.0+0.1*0:2      , getValue(testHc, as.character(0:2)   ))

})

test_that("conversions to data frames", {
  numParams <- 3L
  numPoints <- 4L
  values <- function(i) {1.0 + i*0.1*(0:(numParams-1))}
  pointsHc <- lapply(1:numPoints, FUN= function(i) {
    testHc <- clrNew('CSIRO.Metaheuristics.Tests.TestHyperCube', numParams, 1.0, 0.0, 2.0)
    setValue(testHc, as.character(0:2), values(i))
    return(testHc);
  })
  testClassName <- 'CSIRO.Metaheuristics.R.Pkgs.Tests.TestCases'
  scores <- clrCallStatic(testClassName, 'IdentityScores', pointsHc[[1]], pointsHc[[2]],pointsHc[[3]], pointsHc[[4]])
  scores <- asDataFrame(scores)
  expect_true(is.data.frame(scores))
  expect_equal(numParams *2, ncol(scores))
  
  scoresColNames <- paste0("X",1:numParams-1,'_s')
  for (i in 1:numPoints)
  {
    expect_equal(values(i), as.numeric(scores[i,scoresColNames]))
  }
})

