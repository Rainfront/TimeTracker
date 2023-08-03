import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { Session, TimeMark, TimeResponse } from "../Types/Time";
import { itemsInPage } from "../../Components/TimeStatistic";
import { LocationSlicer } from "./LocationSlice";
import { locationOffset } from "./LocationSlice";
import { ChangeLocationPayload, Location, officeTimeZone } from "./LocationSlice";
import { act } from "@testing-library/react";
import { LocationPayload } from "./LocationSlice";

export type ErrorGraphql = [
    {
        "message":string
        "locations":
        { "line": number, "column": number }[], 
        "path": string[], 
        "extensions": { "code": string, "codes":string}
    }
]

export type statusType = "idle" | "error" | "success" | "loading";

export type UpdateTime = {
    time: Session,
    oldSDate: Date
}

export type stateTimeType = {
    time: TimeResponse,
    error?: string,
    status: statusType
}

const time: TimeResponse = {
    itemsCount: 0,
    time: {
        daySeconds: 0,
        weekSeconds: 0,
        monthSeconds: 0,
        sessions: []
    },
    isStarted: false
}

const initialState: stateTimeType = {
    time,
    status: "idle"
}

export const timeSlicer = createSlice({
    name: "time",
    initialState,
    reducers: {
        setStartTime: (state, action: PayloadAction<Date>) => {

            if (state.time.time.sessions.length == itemsInPage)
                state.time.time.sessions.pop()

            state.time.time.sessions.unshift({
                startTimeTrackDate: action.payload,
                endTimeTrackDate: null,
                timeMark: TimeMark.Day
            })
        },
        setEndTime: (state, action: PayloadAction<Date>) => {
            state.time.time.sessions[0].endTimeTrackDate = action.payload
            const differenceInSeconds = Math.floor((state.time.time.sessions[0].endTimeTrackDate.getTime() - state.time.time.sessions[0].startTimeTrackDate.getTime()) / 1000)
            state.time.time.daySeconds += differenceInSeconds;
            state.time.time.weekSeconds += differenceInSeconds;
            state.time.time.monthSeconds += differenceInSeconds;
        },
        updateTime: (state, action: PayloadAction<UpdateTime>) => {
            state.time.time.sessions = state.time.time.sessions.map((up) => {
                if (up.startTimeTrackDate.toISOString() === action.payload.oldSDate.toISOString()) {

                    const olddifferenceInSeconds = Math.floor((up.endTimeTrackDate!.getTime() - up.startTimeTrackDate.getTime()) / 1000)

                    state.time.time.daySeconds -= olddifferenceInSeconds
                    state.time.time.weekSeconds -= olddifferenceInSeconds
                    state.time.time.monthSeconds -= olddifferenceInSeconds

                    return action.payload.time
                }
                return up
            })

            const newdifferenceInSeconds = Math.floor((action.payload.time.endTimeTrackDate!.getTime() - action.payload.time.startTimeTrackDate.getTime()) / 1000)
            state.time.time.daySeconds += newdifferenceInSeconds
            state.time.time.weekSeconds += newdifferenceInSeconds
            state.time.time.monthSeconds += newdifferenceInSeconds

        },
        setTime: (state, action: PayloadAction<TimeResponse>) => {
            state.time = action.payload;
            state.status = "success"
            state.error = ""
        },
        setloadingStatus: (state) => {
            state.status = "loading";
        },
        setErrorStatusAndError: (state, action: PayloadAction<string>) => {
            state.status = "error";
            state.error = action.payload;
        },
        setIdleStatus: (state) => {
            state.status = "idle"
            state.error = ""
        },
        clearErroMassage: (state) => {
            state.error = ""
        },
        changeTimerState: (state) => {
            state.time.isStarted = !state.time.isStarted;
        }
    },
    extraReducers: {
        [LocationSlicer.actions.changeLocation.type]: (state, action: PayloadAction<ChangeLocationPayload>) => {
            state.time.time.sessions.forEach(v => {
                v.endTimeTrackDate = v.endTimeTrackDate ? new Date(new Date(v.endTimeTrackDate).getTime() + (action.payload.newOffSet - action.payload.oldOffSet) * 60000) : v.endTimeTrackDate
                v.startTimeTrackDate = new Date(new Date(v.startTimeTrackDate).getTime() + (action.payload.newOffSet - action.payload.oldOffSet) * 60000)
            })
        },
        [LocationSlicer.actions.setLocation.type]: (state, action: PayloadAction<LocationPayload>) => {
            state.time.time.sessions.forEach(v => {
                v.endTimeTrackDate = v.endTimeTrackDate ? new Date(new Date(v.endTimeTrackDate).getTime() + (action.payload.userOffset - action.payload.oldOffset) * 60000) : v.endTimeTrackDate
                v.startTimeTrackDate = new Date(new Date(v.startTimeTrackDate).getTime() + (action.payload.userOffset - action.payload.oldOffset) * 60000)
            })
        },
    }
})

export const timeSlicerAction = timeSlicer.actions;
export const { setTime, updateTime, setEndTime, setStartTime, setloadingStatus, changeTimerState, setErrorStatusAndError, setIdleStatus, clearErroMassage } = timeSlicer.actions;
export default timeSlicer.reducer;