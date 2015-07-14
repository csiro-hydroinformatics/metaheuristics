require(mh)

libs <- system.file('libs', package='mh')
dllName <- file.path(libs, "Mh.Samples.dll")
stopifnot(file.exists(dllName))
clrLoadAssembly(dllName)

mtype <- clrGetType("Mh.Samples.Helper")
rosen <- clrCallStatic(mtype, "GetRosenbrockFunction")
clrReflect(rosen)
sysConfig <- clrCallStatic(mtype, "GetRosenbrockParameters")





# create a model simulation
# Note to self: needs more convenience methods in the API
mtype <- clrGetType("TIME.Models.RainfallRunoff.AWBM.AWBM")
if (is.null(mtype)) {stop('Failed to find the model Type')}
mr <- createModelRunner(mtype)

# Set time series inputs
data('catchment-data')
plot(catData)
startDate <- as.Date('1993-01-01')
endDate <- as.Date('1999-01-01')
d <- window(catData, start=startDate, end=endDate)

playTimeSeries(mr, 'rainfall', zooToDailyTts(d[,'Rain']))
playTimeSeries(mr, 'evapotranspiration', zooToDailyTts(d[,'Pet'])) # not right, but for demo only
recordTimeSeries(mr, 'runoff')
# note to self TODO: playTimeSeries(mr, 'rainfall', zooToDailyTts(d['Rain']))  needs check. Note the missing comma in d['Rain'] instead of  d[,'Rain']

# Run it
executeSimulation(simul=mr)

# Plot runoff data
runoff <- getRecordedTimeSeries(mr, 'runoff')
# a workaround to overcome varying date representations...
index(d) <- index(runoff)
z <- merge(runoff, d[,'QObs'])
plot(z, plot.type='single', col=c('red','blue'))

